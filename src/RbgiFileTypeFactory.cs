using PaintDotNet;

namespace RBGIFileTypePlugin;

/// <summary>
/// paint.NET looks for plug-in DLLs that expose IFileTypeFactory.
/// </summary>
public sealed class RbgiFileTypeFactory : IFileTypeFactory
{
    public FileType[] GetFileTypeInstances()
    {
        return new FileType[]
        {
            new RbgiFileType()
        };
    }
}
