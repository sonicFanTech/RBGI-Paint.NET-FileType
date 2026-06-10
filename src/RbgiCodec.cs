using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RBGIFileTypePlugin.Preview52;

internal enum RbgiImageMode : uint
{
    FlatImage = 0,
    RenderBackground = 1,
    SkyBoxSingleTexture = 2,
    SkyBoxCubeTexture = 3
}

internal sealed class RbgiDecodedFile
{
    public bool LegacyRenamedPng { get; init; }
    public ushort Version { get; init; } = 1;
    public uint Width { get; init; }
    public uint Height { get; init; }
    public RbgiImageMode Mode { get; init; } = RbgiImageMode.RenderBackground;
    public uint Flags { get; init; }
    public string MetadataJson { get; init; } = "{}";
    public required byte[] PngBytes { get; init; }

    public static RbgiDecodedFile FromLegacyPng(byte[] pngBytes)
    {
        RbgiPngInfo dimensions = RbgiPngInfo.Read(pngBytes);
        return new RbgiDecodedFile
        {
            LegacyRenamedPng = true,
            Width = dimensions.Width,
            Height = dimensions.Height,
            MetadataJson = "{\"format\":\"Legacy renamed PNG .RBGI\"}",
            PngBytes = pngBytes
        };
    }
}

internal static class RbgiMetadata
{
    public static IReadOnlyDictionary<string, object?> CreateForExport(
        string name,
        string author,
        string description,
        string contentVersion,
        RbgiImageMode mode)
    {
        Dictionary<string, object?> metadata = new()
        {
            ["format"] = "RBGI v1",
            ["name"] = string.IsNullOrWhiteSpace(name) ? "paint.NET Exported RBGI Image" : name.Trim(),
            ["type"] = mode.ToString(),
            ["encoder"] = "RBGI Paint.NET FileType Plugin",
            ["encoderVersion"] = "1.1.0-preview.1"
        };

        AddWhenNotBlank(metadata, "author", author);
        AddWhenNotBlank(metadata, "description", description);
        AddWhenNotBlank(metadata, "contentVersion", contentVersion);
        return metadata;
    }

    private static void AddWhenNotBlank(Dictionary<string, object?> metadata, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            metadata[key] = value.Trim();
        }
    }
}

internal readonly record struct RbgiPngInfo(uint Width, uint Height)
{
    private static ReadOnlySpan<byte> PngSignature => [137, 80, 78, 71, 13, 10, 26, 10];

    public static RbgiPngInfo Read(byte[] pngBytes)
    {
        if (pngBytes.Length < 24 || !pngBytes.AsSpan(0, 8).SequenceEqual(PngSignature))
        {
            throw new InvalidDataException("The embedded image is not a valid PNG file.");
        }

        if (!pngBytes.AsSpan(12, 4).SequenceEqual("IHDR"u8))
        {
            throw new InvalidDataException("The embedded PNG is missing its IHDR chunk.");
        }

        uint width = BinaryPrimitives.ReadUInt32BigEndian(pngBytes.AsSpan(16, 4));
        uint height = BinaryPrimitives.ReadUInt32BigEndian(pngBytes.AsSpan(20, 4));
        if (width == 0 || height == 0)
        {
            throw new InvalidDataException("The embedded PNG has invalid dimensions.");
        }

        return new RbgiPngInfo(width, height);
    }
}

internal static class RbgiCodec
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("RBGI");
    private const ushort CurrentVersion = 1;
    private const uint HeaderSize = 38;
    private const uint MaxMetadataSize = 8u * 1024u * 1024u;
    private const ulong MaxPngSize = 512ul * 1024ul * 1024ul;

    public static bool LooksLikeRealRbgi(byte[] bytes)
    {
        return bytes.Length >= 4 && bytes.AsSpan(0, 4).SequenceEqual(Magic);
    }

    public static RbgiDecodedFile Decode(byte[] bytes)
    {
        if (bytes.Length < HeaderSize)
        {
            throw new InvalidDataException("The file is too small to be a real RBGI v1 file.");
        }

        if (!LooksLikeRealRbgi(bytes))
        {
            throw new InvalidDataException("Missing RBGI magic header.");
        }

        ReadOnlySpan<byte> span = bytes;
        ushort version = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(4, 2));
        uint headerSize = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(6, 4));
        uint width = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(10, 4));
        uint height = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(14, 4));
        uint modeRaw = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(18, 4));
        uint flags = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(22, 4));
        ulong pngSize = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(26, 8));
        uint metadataSize = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(34, 4));

        if (version != CurrentVersion) throw new InvalidDataException($"Unsupported RBGI version: {version}.");
        if (headerSize < HeaderSize) throw new InvalidDataException("Invalid RBGI header size.");
        if (metadataSize > MaxMetadataSize) throw new InvalidDataException("RBGI metadata is too large.");
        if (pngSize > MaxPngSize) throw new InvalidDataException("Embedded PNG data is too large.");

        ulong metadataOffset = headerSize;
        ulong pngOffset = metadataOffset + metadataSize;
        ulong requiredLength = pngOffset + pngSize;
        if (requiredLength > (ulong)bytes.Length) throw new InvalidDataException("The RBGI file is truncated or corrupt.");
        if (metadataOffset > int.MaxValue || pngOffset > int.MaxValue || pngSize > int.MaxValue)
            throw new InvalidDataException("The RBGI file is too large for this plug-in build.");

        string metadataJson = "{}";
        if (metadataSize > 0)
        {
            metadataJson = Encoding.UTF8.GetString(span.Slice((int)metadataOffset, (int)metadataSize));
            using JsonDocument json = JsonDocument.Parse(metadataJson);
            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("RBGI metadata JSON must be an object.");
        }

        byte[] pngBytes = span.Slice((int)pngOffset, (int)pngSize).ToArray();
        RbgiPngInfo pngInfo = RbgiPngInfo.Read(pngBytes);
        if (width != 0 && height != 0 && (width != pngInfo.Width || height != pngInfo.Height))
            throw new InvalidDataException("The RBGI header dimensions do not match the embedded PNG dimensions.");

        return new RbgiDecodedFile
        {
            LegacyRenamedPng = false,
            Version = version,
            Width = pngInfo.Width,
            Height = pngInfo.Height,
            Mode = Enum.IsDefined(typeof(RbgiImageMode), modeRaw) ? (RbgiImageMode)modeRaw : RbgiImageMode.RenderBackground,
            Flags = flags,
            MetadataJson = metadataJson,
            PngBytes = pngBytes
        };
    }

    public static void Encode(Stream output, byte[] pngBytes, RbgiImageMode mode, IReadOnlyDictionary<string, object?> metadata)
    {
        if (pngBytes.Length == 0) throw new InvalidDataException("No PNG bytes were provided for the RBGI file.");
        RbgiPngInfo pngInfo = RbgiPngInfo.Read(pngBytes);
        byte[] metadataBytes = JsonSerializer.SerializeToUtf8Bytes(metadata, new JsonSerializerOptions { WriteIndented = false });
        if (metadataBytes.Length > MaxMetadataSize) throw new InvalidDataException("RBGI metadata is too large.");

        using BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true);
        writer.Write(Magic);
        writer.Write(CurrentVersion);
        writer.Write(HeaderSize);
        writer.Write(pngInfo.Width);
        writer.Write(pngInfo.Height);
        writer.Write((uint)mode);
        writer.Write((uint)0);
        writer.Write((ulong)pngBytes.Length);
        writer.Write((uint)metadataBytes.Length);
        writer.Write(metadataBytes);
        writer.Write(pngBytes);
    }
}
