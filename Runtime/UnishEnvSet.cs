using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishEnvSet : IUnishResource
    {
        public readonly IUnishEnv BuiltIn;
        public readonly IUnishEnv Environment;
        public readonly IUnishEnv Shell;

        public UnishEnvSet(IUnishEnv builtIn, IUnishEnv environment, IUnishEnv shell)
        {
            BuiltIn     = builtIn;
            Environment = environment;
            Shell       = shell;
        }

        public UnishEnvSet Fork()
        {
            return new UnishEnvSet(BuiltIn.Fork(), Environment.Fork(), Shell.Fork());
        }

        public async UniTask InitializeAsync()
        {
            await BuiltIn.InitializeAsync();
            await Environment.InitializeAsync();
            await Shell.InitializeAsync();
        }

        public async UniTask FinalizeAsync()
        {
            await Shell.FinalizeAsync();
            await Environment.FinalizeAsync();
            await BuiltIn.FinalizeAsync();
        }
    }
}
