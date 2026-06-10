using PaintDotNet.FileTypes;
using Pdn52 = PaintDotNet.FileTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace RBGIFileTypePlugin.Preview52;

/// <summary>
/// Uses Paint.NET 5.2's built-in PNG FileType for the PNG payload embedded in an
/// RBGI container. This avoids System.Drawing and delegates PNG decoding and
/// encoding to Paint.NET itself.
/// </summary>
internal static class RbgiPngBridge
{
    public static Pdn52.IFileTypeDocument Load(
        IServiceProvider services,
        Pdn52.IFileTypeDocumentFactory factory,
        Stream input)
    {
        Pdn52.IPngFileType pngFileType = CreatePngFileType(services);

        if (TryInvokeLoad(pngFileType, factory, input, out Pdn52.IFileTypeDocument? document))
        {
            return document;
        }

        object? loader = TryCreatePipelinePart(
            pngFileType,
            "CreateLoader",
            "CreateFileTypeLoader",
            "GetLoader");

        if (loader is not null &&
            TryInvokeLoad(loader, factory, input, out document))
        {
            return document;
        }

        throw new MissingMethodException(
            "Could not find a compatible Paint.NET 5.2 PNG loading entry point. " +
            DescribeRelevantMethods(pngFileType, loader));
    }

    public static void Save(
        IServiceProvider services,
        Pdn52.IReadOnlyFileTypeDocument document,
        Stream output,
        PaintDotNet.ProgressEventHandler progressCallback)
    {
        Pdn52.IPngFileType pngFileType = CreatePngFileType(services);

        if (TryInvokeSave(pngFileType, pngFileType, document, output, progressCallback))
        {
            return;
        }

        object? saver = TryCreatePipelinePart(
            pngFileType,
            "CreateSaver",
            "CreateFileTypeSaver",
            "GetSaver");

        if (saver is not null &&
            TryInvokeSave(saver, pngFileType, document, output, progressCallback))
        {
            return;
        }

        throw new MissingMethodException(
            "Could not find a compatible Paint.NET 5.2 PNG saving entry point. " +
            DescribeRelevantMethods(pngFileType, saver));
    }

    private static Pdn52.IPngFileType CreatePngFileType(IServiceProvider services)
    {
        Pdn52.IFileTypesService fileTypesService =
            services.GetService(typeof(Pdn52.IFileTypesService)) as Pdn52.IFileTypesService
            ?? throw new InvalidOperationException("Paint.NET did not provide IFileTypesService.");

        // This is the API path recommended by Rick Brewster for Paint.NET 5.2.
        return fileTypesService.CreatePngFileType()
            ?? throw new InvalidOperationException("Paint.NET did not provide a PNG FileType instance.");
    }

    private static bool TryInvokeLoad(
        object target,
        Pdn52.IFileTypeDocumentFactory factory,
        Stream input,
        out Pdn52.IFileTypeDocument? document)
    {
        foreach (MethodInfo method in GetCandidateMethods(target, "Load")
            .OrderByDescending(static m => m.GetParameters().Length))
        {
            if (!TryBuildLoadArguments(method.GetParameters(), factory, input, out object?[]? arguments))
            {
                continue;
            }

            object? result = InvokeAndUnwrap(method, target, arguments);
            if (result is Pdn52.IFileTypeDocument loadedDocument)
            {
                document = loadedDocument;
                return true;
            }
        }

        document = null;
        return false;
    }

    private static bool TryBuildLoadArguments(
        ParameterInfo[] parameters,
        Pdn52.IFileTypeDocumentFactory factory,
        Stream input,
        out object?[]? arguments)
    {
        arguments = new object?[parameters.Length];
        bool suppliedInput = false;

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            Type type = parameter.ParameterType;

            if (!suppliedInput && typeof(Stream).IsAssignableFrom(type))
            {
                arguments[i] = input;
                suppliedInput = true;
            }
            else if (type.IsInstanceOfType(factory))
            {
                arguments[i] = factory;
            }
            else if (parameter.HasDefaultValue)
            {
                arguments[i] = parameter.DefaultValue;
            }
            else
            {
                arguments = null;
                return false;
            }
        }

        return suppliedInput;
    }

    private static bool TryInvokeSave(
        object target,
        Pdn52.IPngFileType pngFileType,
        Pdn52.IReadOnlyFileTypeDocument document,
        Stream output,
        PaintDotNet.ProgressEventHandler progressCallback)
    {
        foreach (MethodInfo method in GetCandidateMethods(target, "Save")
            .OrderByDescending(static m => m.GetParameters().Length))
        {
            if (!TryBuildSaveArguments(
                target,
                pngFileType,
                method.GetParameters(),
                document,
                output,
                progressCallback,
                out object?[]? arguments))
            {
                continue;
            }

            InvokeAndUnwrap(method, target, arguments);
            return true;
        }

        return false;
    }

    private static bool TryBuildSaveArguments(
        object target,
        Pdn52.IPngFileType pngFileType,
        ParameterInfo[] parameters,
        Pdn52.IReadOnlyFileTypeDocument document,
        Stream output,
        PaintDotNet.ProgressEventHandler progressCallback,
        out object?[]? arguments)
    {
        arguments = new object?[parameters.Length];
        bool suppliedDocument = false;
        bool suppliedOutput = false;

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            Type type = parameter.ParameterType;

            if (!suppliedOutput && typeof(Stream).IsAssignableFrom(type))
            {
                arguments[i] = output;
                suppliedOutput = true;
            }
            else if (!suppliedDocument && type.IsInstanceOfType(document))
            {
                arguments[i] = document;
                suppliedDocument = true;
            }
            else if (type.IsInstanceOfType(progressCallback))
            {
                arguments[i] = progressCallback;
            }
            else if (TryCreateDefaultSaveOptions(target, pngFileType, type, out object? options))
            {
                arguments[i] = options;
            }
            else if (parameter.HasDefaultValue)
            {
                arguments[i] = parameter.DefaultValue;
            }
            else
            {
                arguments = null;
                return false;
            }
        }

        return suppliedDocument && suppliedOutput;
    }

    private static bool TryCreateDefaultSaveOptions(
        object target,
        Pdn52.IPngFileType pngFileType,
        Type expectedType,
        out object? options)
    {
        foreach (object candidateTarget in new object[] { pngFileType, target })
        {
            foreach (MethodInfo method in GetAllCandidateMethods(candidateTarget)
                .Where(static m => m.GetParameters().Length == 0)
                .Where(m => expectedType.IsAssignableFrom(m.ReturnType))
                .Where(static m =>
                    m.Name.Contains("SaveOptions", StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains("SaveToken", StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains("DefaultOptions", StringComparison.OrdinalIgnoreCase)))
            {
                options = InvokeAndUnwrap(method, candidateTarget, null);
                return options is not null || !expectedType.IsValueType;
            }

            foreach (PropertyInfo property in GetAllCandidateProperties(candidateTarget)
                .Where(p => p.GetIndexParameters().Length == 0)
                .Where(p => expectedType.IsAssignableFrom(p.PropertyType))
                .Where(static p =>
                    p.Name.Contains("SaveOptions", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("DefaultOptions", StringComparison.OrdinalIgnoreCase)))
            {
                options = property.GetValue(candidateTarget);
                return options is not null || !expectedType.IsValueType;
            }
        }

        if (expectedType.IsValueType)
        {
            options = Activator.CreateInstance(expectedType);
            return true;
        }

        options = null;
        return false;
    }

    private static object? TryCreatePipelinePart(object target, params string[] names)
    {
        foreach (string name in names)
        {
            foreach (MethodInfo method in GetCandidateMethods(target, name)
                .Where(static m => m.GetParameters().Length == 0))
            {
                object? result = InvokeAndUnwrap(method, target, null);
                if (result is not null)
                {
                    return result;
                }
            }

            foreach (PropertyInfo property in GetAllCandidateProperties(target)
                .Where(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(p.Name, name.Replace("Create", string.Empty, StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase))
                .Where(static p => p.GetIndexParameters().Length == 0))
            {
                object? result = property.GetValue(target);
                if (result is not null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    private static IEnumerable<MethodInfo> GetCandidateMethods(object target, string simpleName)
    {
        return GetAllCandidateMethods(target)
            .Where(m => string.Equals(GetSimpleMethodName(m.Name), simpleName, StringComparison.Ordinal));
    }

    private static IEnumerable<MethodInfo> GetAllCandidateMethods(object target)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        Type concreteType = target.GetType();

        foreach (Type type in new[] { concreteType }.Concat(concreteType.GetInterfaces()))
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                string key = $"{method.DeclaringType?.FullName}|{method}";
                if (seen.Add(key))
                {
                    yield return method;
                }
            }
        }
    }

    private static IEnumerable<PropertyInfo> GetAllCandidateProperties(object target)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        Type concreteType = target.GetType();

        foreach (Type type in new[] { concreteType }.Concat(concreteType.GetInterfaces()))
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                string key = $"{property.DeclaringType?.FullName}|{property}";
                if (seen.Add(key))
                {
                    yield return property;
                }
            }
        }
    }

    private static string GetSimpleMethodName(string name)
    {
        int separator = name.LastIndexOf('.');
        return separator >= 0 ? name[(separator + 1)..] : name;
    }

    private static object? InvokeAndUnwrap(MethodInfo method, object target, object?[]? arguments)
    {
        try
        {
            return method.Invoke(target, arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static string DescribeRelevantMethods(object first, object? second)
    {
        IEnumerable<object> targets = second is null ? new[] { first } : new[] { first, second };
        string methods = string.Join(
            "; ",
            targets.SelectMany(GetAllCandidateMethods)
                .Where(static m =>
                    m.Name.Contains("Load", StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains("Save", StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains("Loader", StringComparison.OrdinalIgnoreCase) ||
                    m.Name.Contains("Saver", StringComparison.OrdinalIgnoreCase))
                .Select(static m => $"{m.DeclaringType?.FullName}.{m}")
                .Distinct(StringComparer.Ordinal)
                .Take(80));

        return string.IsNullOrWhiteSpace(methods)
            ? "No relevant PNG methods were visible."
            : "Visible PNG methods: " + methods;
    }
}
