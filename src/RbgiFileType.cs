using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PaintDotNet;

namespace RBGIFileTypePlugin;

internal sealed class RbgiFileType : FileType
{
    public RbgiFileType()
        : base(
            "RBGI Render Background Image",
            new FileTypeOptions
            {
                LoadExtensions = new string[] { ".rbgi" },
                SaveExtensions = new string[] { ".rbgi" }
            })
    {
    }

    protected override Document OnLoad(Stream input)
    {
        byte[] fileBytes = ReadAllBytes(input);

        RbgiDecodedFile decoded;
        if (RbgiCodec.LooksLikeRealRbgi(fileBytes))
        {
            decoded = RbgiCodec.Decode(fileBytes);
        }
        else
        {
            // Legacy support: older .RBGI files were just PNG bytes with a custom extension.
            decoded = RbgiDecodedFile.FromLegacyPng(fileBytes);
        }

        using MemoryStream pngStream = new MemoryStream(decoded.PngBytes, writable: false);
        using Image image = Image.FromStream(pngStream);

        return Document.FromImage(image);
    }

    protected override void OnSave(
        Document input,
        Stream output,
        SaveConfigToken token,
        Surface scratchSurface,
        ProgressEventHandler progressCallback)
    {
        scratchSurface.Fill(ColorBgra.Transparent);
        input.Flatten(scratchSurface);

        byte[] pngBytes;
        using (Bitmap bitmap = scratchSurface.CreateAliasedBitmap())
        using (MemoryStream pngStream = new MemoryStream())
        {
            bitmap.Save(pngStream, ImageFormat.Png);
            pngBytes = pngStream.ToArray();
        }

        RbgiCodec.Encode(
            output,
            pngBytes,
            width: scratchSurface.Width,
            height: scratchSurface.Height,
            mode: RbgiImageMode.RenderBackground,
            metadata: RbgiMetadata.CreateDefault());
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        if (stream is MemoryStream memoryStream)
        {
            return memoryStream.ToArray();
        }

        using MemoryStream copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }
}
