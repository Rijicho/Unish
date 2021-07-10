using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishPresenter
    {
        IUnishIO                IO                { get; }
        IUnishCommandRepository CommandRepository { get; }
        IUnishCommandRunner     CommandRunner     { get; }
        IUnishColorParser       ColorParser       { get; }
        IUnishTimeProvider      TimeProvider      { get; }
        IUnishRcRepository      RcRepository      { get; }
        IUnishDirectoryRoot     Directory         { get; }
        string                  Prompt            { get; set; }

        UniTask RunAsync();
        void Halt();
    }
}
