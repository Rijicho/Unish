using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public static class UnishResourceWithEnvExtensions
    {
        public static UniTask InitializeAsync(this IUnishResourceWithEnv resource, IUnishEnv globalEnv)
        {
            resource.GlobalEnv = globalEnv;
            return resource.InitializeAsync();
        }
    }
}
