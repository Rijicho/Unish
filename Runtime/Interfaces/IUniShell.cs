using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUniShell : IUnishProcess
    {
        UniTask RunAsync();
        void Halt();
    }
}
