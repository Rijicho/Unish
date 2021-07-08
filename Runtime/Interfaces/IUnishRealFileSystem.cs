namespace RUtil.Debug.Shell
{
    public interface IUnishRealFileSystem : IUnishDirectorySystem
    {
        string RealHomePath { get; }
    }
}
