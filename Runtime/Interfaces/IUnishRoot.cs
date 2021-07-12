using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishRoot : IUniShell
    {
        IUnishEnv GlobalEnv { get; }
    }
}
