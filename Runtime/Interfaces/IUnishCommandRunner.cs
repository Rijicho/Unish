using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishCommandRunner : IUnishResource
    {
        UniTask RunCommandAsync(IUnishPresenter shell, string cmd);
    }
}
