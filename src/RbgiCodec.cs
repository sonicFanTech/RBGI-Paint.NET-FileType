using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RBGIFileTypePlugin;

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
        return new RbgiDecodedFile
        {
            LegacyRenamedPng = true,
            MetadataJson = "{\"format\":\"Legacy renamed PNG .RBGI\"}",
            PngBytes = pngBytes
        };
    }
}

internal static class RbgiMetadata
{
    public static IReadOnlyDictionary<string, object?> CreateDefault()
    {
        return new Dictionary<string, object?>
        {
            ["format"] = "RBGI v1",
            ["name"] = "paint.NET Exported RBGI Image",
            ["type"] = "RenderBackgroundImage",
            ["encoder"] = "RBGI paint.NET FileType Plugin",
            ["encoderVersion"] = "1.0.0"
        };
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
        return bytes.Length >= 4 &&
               bytes[0] == Magic[0] &&
               bytes[1] == Magic[1] &&
               bytes[2] == Magic[2] &&
               bytes[3] == Magic[3];
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

        if (version != CurrentVersion)
        {
            throw new InvalidDataException($"Unsupported RBGI version: {version}.");
        }

        if (headerSize < HeaderSize)
        {
            throw new InvalidDataException("Invalid RBGI header size.");
        }

        if (metadataSize > MaxMetadataSize)
        {
            throw new InvalidDataException("RBGI metadata is too large.");
        }

        if (pngSize > MaxPngSize)
        {
            throw new InvalidDataException("Embedded PNG data is too large.");
        }

        ulong metadataOffset = headerSize;
        ulong pngOffset = metadataOffset + metadataSize;
        ulong requiredLength = pngOffset + pngSize;

        if (requiredLength > (ulong)bytes.Length)
        {
            throw new InvalidDataException("The RBGI file is truncated or corrupt.");
        }

        string metadataJson = "{}";
        if (metadataSize > 0)
        {
            byte[] metadataBytes = span.Slice((int)metadataOffset, (int)metadataSize).ToArray();
            metadataJson = Encoding.UTF8.GetString(metadataBytes);

            // Validate that the metadata is actually JSON, just like the C++ SDK does.
            using JsonDocument json = JsonDocument.Parse(metadataJson);
            if (json.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("RBGI metadata JSON must be an object.");
            }
        }

        byte[] pngBytes = span.Slice((int)pngOffset, (int)pngSize).ToArray();

        return new RbgiDecodedFile
        {
            LegacyRenamedPng = false,
            Version = version,
            Width = width,
            Height = height,
            Mode = Enum.IsDefined(typeof(RbgiImageMode), modeRaw)
                ? (RbgiImageMode)modeRaw
                : RbgiImageMode.RenderBackground,
            Flags = flags,
            MetadataJson = metadataJson,
            PngBytes = pngBytes
        };
    }

    public static void Encode(
        Stream output,
        byte[] pngBytes,
        int width,
        int height,
        RbgiImageMode mode,
        IReadOnlyDictionary<string, object?> metadata)
    {
        if (pngBytes.Length == 0)
        {
            throw new InvalidDataException("No PNG bytes were provided for the RBGI file.");
        }

        byte[] metadataBytes = JsonSerializer.SerializeToUtf8Bytes(metadata, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        if (metadataBytes.Length > MaxMetadataSize)
        {
            throw new InvalidDataException("RBGI metadata is too large.");
        }

        using BinaryWriter writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);

        writer.Write(Magic);
        writer.Write(CurrentVersion);
        writer.Write(HeaderSize);
        writer.Write((uint)width);
        writer.Write((uint)height);
        writer.Write((uint)mode);
        writer.Write((uint)0); // flags reserved for future versions
        writer.Write((ulong)pngBytes.Length);
        writer.Write((uint)metadataBytes.Length);
        writer.Write(metadataBytes);
        writer.Write(pngBytes);
    }
}
