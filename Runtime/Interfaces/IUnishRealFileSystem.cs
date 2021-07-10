namespace RUtil.Debug.Shell
{
    public interface IUnishRealFileSystem : IUnishDirectoryHome
    {
        string RealHomePath { get; }
    }
}
