using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class Unish
    {
        private UnishEnvSet         mEnv;
        private IUnishIO            mIO;
        private IUnishInterpreter   mInterpreter;
        private IUnishDirectoryRoot mDirectory;
        private IUniShell           mMainShell;
        private bool                mIsUprofileExecuted;

        // ----------------------------------
        // public methods
        // ----------------------------------

        public Unish(
            UnishEnvSet env = default,
            IUnishIO io = default,
            IUnishInterpreter interpreter = default,
            IUnishDirectoryRoot directory = default)
        {
            mEnv         = env ?? new UnishEnvSet(new BuiltinEnv(), new GlobalEnv(), new ShellEnv());
            mIO          = io ?? new DefaultIO();
            mInterpreter = interpreter ?? new DefaultInterpreter();
            mDirectory   = directory ?? new DefaultDirectoryRoot();
            mMainShell   = new UnishCore(mEnv, mIO, mInterpreter, mDirectory, null);
        }

        public void Run()
        {
            RunAsync().Forget();
        }

        public async UniTask RunAsync()
        {
            await Init();
            await mMainShell.RunAsync();
            await Quit();
        }

        public void Halt()
        {
            mMainShell.Halt();
        }


        // ----------------------------------
        // protected methods
        // ----------------------------------

        protected virtual UniTask OnPreInitAsync()
        {
            return default;
        }

        protected virtual UniTask OnPostInitAsync()
        {
            return default;
        }

        protected virtual UniTask OnPreQuitAsync()
        {
            return default;
        }

        protected virtual UniTask OnPostQuitAsync()
        {
            return default;
        }


        // ----------------------------------
        // private methods
        // ----------------------------------


        private async UniTask Init()
        {
            await OnPreInitAsync();
            await mEnv.InitializeAsync();
            await mIO.InitializeAsync(mEnv.BuiltIn);
            mIO.OnHaltInput += Halt;
            await mDirectory.InitializeAsync(mEnv.BuiltIn);
            await mInterpreter.InitializeAsync(mEnv.BuiltIn);
            await RunBuiltInProfile();
            await RunUserProfiles();
            await OnPostInitAsync();
        }

        private async UniTask Quit()
        {
            await OnPreQuitAsync();
            await mInterpreter.FinalizeAsync();
            await mDirectory.FinalizeAsync();
            mIO.OnHaltInput -= Halt;
            await mIO.FinalizeAsync();
            await mEnv.FinalizeAsync();
            mEnv         = null;
            mDirectory   = null;
            mIO          = null;
            mInterpreter = null;
            mMainShell   = null;
            await OnPostQuitAsync();
        }

        protected virtual UniTask RunBuiltInProfile()
        {
            if (mEnv.BuiltIn.TryGet(UnishBuiltInEnvKeys.HomePath, out string homePath))
            {
                mDirectory.TryChangeDirectory(homePath);
            }

            return default;
        }


        private async UniTask RunUserProfiles()
        {
            var profile = mEnv.BuiltIn[UnishBuiltInEnvKeys.ProfilePath].S;
            var rc      = mEnv.BuiltIn[UnishBuiltInEnvKeys.RcPath].S;
            if (!mIsUprofileExecuted)
            {
                if (mDirectory.TryFindEntry(profile, out _))
                {
                    await foreach (var c in mDirectory.ReadLines(profile))
                    {
                        await mInterpreter.RunCommandAsync(mMainShell, c);
                    }
                }

                mIsUprofileExecuted = true;
            }

            if (mDirectory.TryFindEntry(rc, out _))
            {
                await foreach (var c in mDirectory.ReadLines(rc))
                {
                    await mInterpreter.RunCommandAsync(mMainShell, c);
                }
            }
        }
    }
}
