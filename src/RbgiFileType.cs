using Pdn52 = PaintDotNet.FileTypes;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace RBGIFileTypePlugin.Preview52;

public sealed class RbgiFileTypePlugin : Pdn52.PropertyBasedFileType
{
    private enum PropertyNames
    {
        Mode,
        Name,
        Author,
        Description,
        ContentVersion,
        GitHubLink,
        PluginVersion
    }

    public RbgiFileTypePlugin(Pdn52.IFileTypeHost host)
        : base(
            host,
            "RBGI Render Background Image",
            Pdn52.FileTypeOptions.Create() with
            {
                LoadExtensions = [".rbgi"],
                SaveExtensions = [".rbgi"],
                SupportsSavingLayers = false,
                IsSavingConfigurable = true,
                SupportsCancellationExceptions = true,
            })
    {
    }

    protected override Pdn52.PropertyBasedFileTypeSaver OnCreatePropertyBasedSaver() => new Saver(this);
    protected override Pdn52.PropertyBasedFileTypeLoader OnCreatePropertyBasedLoader() => new Loader(this);

    private sealed class Saver : Pdn52.PropertyBasedFileTypeSaver
    {
        public Saver(RbgiFileTypePlugin fileType) : base(fileType) { }

        protected override PropertyCollection OnCreateDefaultSaveProperties()
        {
            Property[] properties =
            [
                StaticListChoiceProperty.CreateForEnum(PropertyNames.Mode, RbgiImageMode.RenderBackground),
                new StringProperty(PropertyNames.Name, "paint.NET Exported RBGI Image"),
                new StringProperty(PropertyNames.Author, string.Empty),
                new StringProperty(PropertyNames.Description, string.Empty),
                new StringProperty(PropertyNames.ContentVersion, string.Empty),
                new UriProperty(PropertyNames.GitHubLink, new Uri("https://github.com/sonicFanTech/RBGI-Paint.NET-FileType")),
                new StringProperty(PropertyNames.PluginVersion),
            ];

            return new PropertyCollection(properties);
        }

        protected override ControlInfo OnCreateSaveOptionsUI(PropertyCollection properties)
        {
            ControlInfo ui = CreateDefaultSaveOptionsUI(properties);

            PropertyControlInfo mode = ui.FindControlForPropertyName(PropertyNames.Mode)!;
            mode.ControlProperties[ControlInfoPropertyNames.DisplayName]!.Value = "RBGI image mode";
            mode.SetValueDisplayName(RbgiImageMode.FlatImage, "Flat image");
            mode.SetValueDisplayName(RbgiImageMode.RenderBackground, "Render background");
            mode.SetValueDisplayName(RbgiImageMode.SkyBoxSingleTexture, "Skybox: single texture");
            mode.SetValueDisplayName(RbgiImageMode.SkyBoxCubeTexture, "Skybox: cube texture");

            SetDisplayName(ui, PropertyNames.Name, "Image name");
            SetDisplayName(ui, PropertyNames.Author, "Author");
            SetDisplayName(ui, PropertyNames.Description, "Description");
            SetDisplayName(ui, PropertyNames.ContentVersion, "Content version");

            PropertyControlInfo github = ui.FindControlForPropertyName(PropertyNames.GitHubLink)!;
            github.ControlProperties[ControlInfoPropertyNames.DisplayName]!.Value = "Project page";
            github.ControlProperties[ControlInfoPropertyNames.Description]!.Value = "GitHub";

            PropertyControlInfo version = ui.FindControlForPropertyName(PropertyNames.PluginVersion)!;
            version.ControlType.Value = PropertyControlType.Label;
            version.ControlProperties[ControlInfoPropertyNames.DisplayName]!.Value = string.Empty;
            version.ControlProperties[ControlInfoPropertyNames.Description]!.Value = "RBGI FileType Preview 5.2 migration candidate v1.1.0-preview.5";
            return ui;
        }

        protected override void OnSave(Pdn52.IPropertyBasedFileTypeSaveContext context)
        {
            Pdn52.IPropertyBasedFileTypeSaveOptions options = context.Options;
            RbgiImageMode mode = (RbgiImageMode)options.GetProperty(PropertyNames.Mode)!.Value!;
            string name = options.GetProperty<StringProperty>(PropertyNames.Name)!.Value;
            string author = options.GetProperty<StringProperty>(PropertyNames.Author)!.Value;
            string description = options.GetProperty<StringProperty>(PropertyNames.Description)!.Value;
            string contentVersion = options.GetProperty<StringProperty>(PropertyNames.ContentVersion)!.Value;

            using MemoryStream png = new();
            RbgiPngBridge.Save(this.Services, context.Document, png, context.ProgressCallback);

            IReadOnlyDictionary<string, object?> metadata = RbgiMetadata.CreateForExport(
                name,
                author,
                description,
                contentVersion,
                mode);

            RbgiCodec.Encode(context.Output, png.ToArray(), mode, metadata);
        }

        private static void SetDisplayName(ControlInfo ui, PropertyNames property, string text)
        {
            PropertyControlInfo control = ui.FindControlForPropertyName(property)!;
            control.ControlProperties[ControlInfoPropertyNames.DisplayName]!.Value = text;
            control.ControlProperties[ControlInfoPropertyNames.Description]!.Value = string.Empty;
        }
    }

    private sealed class Loader : Pdn52.PropertyBasedFileTypeLoader
    {
        public Loader(RbgiFileTypePlugin fileType) : base(fileType) { }

        protected override Pdn52.IFileTypeDocument OnLoad(Pdn52.IPropertyBasedFileTypeLoadContext context)
        {
            byte[] fileBytes = ReadAllBytes(context.Input);
            RbgiDecodedFile decoded = RbgiCodec.LooksLikeRealRbgi(fileBytes)
                ? RbgiCodec.Decode(fileBytes)
                : RbgiDecodedFile.FromLegacyPng(fileBytes);

            using MemoryStream png = new(decoded.PngBytes, writable: false);
            return RbgiPngBridge.Load(this.Services, context.Factory, png);
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            if (stream is MemoryStream memoryStream) return memoryStream.ToArray();
            using MemoryStream copy = new();
            stream.CopyTo(copy);
            return copy.ToArray();
        }
    }
}
