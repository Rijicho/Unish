using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class Unish
    {
        private UnishEnvSet          mEnv;
        private IUnishIO             mIO;
        private IUnishInterpreter    mInterpreter;
        private IUnishFileSystemRoot mFileSystem;
        private IUniShell            mMainShell;
        private bool                 mIsUprofileExecuted;

        // ----------------------------------
        // public methods
        // ----------------------------------

        public Unish(
            UnishEnvSet env = default,
            IUnishIO io = default,
            IUnishInterpreter interpreter = default,
            IUnishFileSystemRoot fileSystem = default)
        {
            mEnv         = env ?? new UnishEnvSet(new BuiltinEnv(), new GlobalEnv(), new ShellEnv());
            mIO          = io ?? new DefaultIO();
            mInterpreter = interpreter ?? new DefaultInterpreter();
            mFileSystem  = fileSystem ?? new UnishFileSystemRoot();
            mMainShell   = new UnishCore(mEnv, mIO, mInterpreter, mFileSystem, null);
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
            await mFileSystem.InitializeAsync(mEnv.BuiltIn);
            await mInterpreter.InitializeAsync(mEnv.BuiltIn);
            await RunBuiltInProfile();
            await RunUserProfiles();
            await OnPostInitAsync();
        }

        private async UniTask Quit()
        {
            await OnPreQuitAsync();
            await mInterpreter.FinalizeAsync();
            await mFileSystem.FinalizeAsync();
            mIO.OnHaltInput -= Halt;
            await mIO.FinalizeAsync();
            await mEnv.FinalizeAsync();
            mEnv         = null;
            mFileSystem  = null;
            mIO          = null;
            mInterpreter = null;
            mMainShell   = null;
            await OnPostQuitAsync();
        }

        protected virtual UniTask RunBuiltInProfile()
        {
            mEnv.BuiltIn.Set(UnishBuiltInEnvKeys.WorkingDirectory, UnishPathConstants.Root);
            if (!mEnv.BuiltIn.TryGet(UnishBuiltInEnvKeys.HomePath, out string homePath))
            {
                mEnv.BuiltIn.Set(UnishBuiltInEnvKeys.HomePath, UnishPathConstants.Root);
            }

            if (mFileSystem.TryFindEntry(homePath, out var home) && home.IsDirectory)
            {
                mEnv.BuiltIn.Set(UnishBuiltInEnvKeys.WorkingDirectory, home.Path);
            }

            return default;
        }


        private async UniTask RunUserProfiles()
        {
            var profile = mEnv.BuiltIn[UnishBuiltInEnvKeys.ProfilePath].S;
            var rc      = mEnv.BuiltIn[UnishBuiltInEnvKeys.RcPath].S;
            if (!mIsUprofileExecuted)
            {
                if (mFileSystem.TryFindEntry(profile, out _))
                {
                    await foreach (var c in mFileSystem.ReadLines(profile))
                    {
                        await mInterpreter.RunCommandAsync(mMainShell, c);
                    }
                }

                mIsUprofileExecuted = true;
            }

            if (mFileSystem.TryFindEntry(rc, out _))
            {
                await foreach (var c in mFileSystem.ReadLines(rc))
                {
                    await mInterpreter.RunCommandAsync(mMainShell, c);
                }
            }
        }
    }
}
