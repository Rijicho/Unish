using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishCommandRunner
    {
        UniTask RunCommandAsync(IUnishPresenter shell, string cmd);
    }
}
