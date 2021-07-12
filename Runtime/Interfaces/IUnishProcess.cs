using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishProcess
    {
        IUnishProcess Parent { get; }
        void Halt();
    }
}
