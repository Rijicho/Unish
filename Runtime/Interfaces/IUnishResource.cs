using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishResource
    {
        UniTask InitializeAsync(IUnishEnv env);
        UniTask FinalizeAsync(IUnishEnv env);
    }
}
