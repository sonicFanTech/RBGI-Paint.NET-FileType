using Pdn52 = PaintDotNet.FileTypes;

namespace RBGIFileTypePlugin.Preview52;

/// <summary>
/// Entry point used by Paint.NET 5.2's new FileType plug-in system.
/// </summary>
public sealed class RbgiFileTypeFactory : Pdn52.IFileTypeFactory
{
    public Pdn52.IFileType[] CreateFileTypes(Pdn52.IFileTypeHost host)
    {
        return [new RbgiFileTypePlugin(host)];
    }
}
