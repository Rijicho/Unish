using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class Unish
    {
        private UnishEnvSet          mEnv;
        private IUnishTerminal       mTerminal;
        private IUnishInterpreter    mInterpreter;
        private IUnishFileSystemRoot mFileSystem;
        private IUniShell            mTerminalShell;
        private bool                 mIsUprofileExecuted;

        // ----------------------------------
        // public methods
        // ----------------------------------

        public Unish(
            UnishEnvSet env = default,
            IUnishTerminal terminal = default,
            IUnishInterpreter interpreter = default,
            IUnishFileSystemRoot fileSystem = default)
        {
            mEnv         = env ?? new UnishEnvSet(new BuiltinEnv(), new GlobalEnv(), new ShellEnv());
            mTerminal    = terminal ?? new DefaultTerminal();
            mInterpreter = interpreter ?? new DefaultInterpreter();
            mFileSystem  = fileSystem ?? new UnishFileSystemRoot();
            var fds = new UnishIOs(mTerminal.ReadAsync, mTerminal.WriteAsync, mTerminal.WriteErrorAsync);
            mTerminalShell = new UnishCore(mEnv, fds, mInterpreter, mFileSystem, null);
        }

        public void Run()
        {
            RunAsync().Forget();
        }

        public async UniTask RunAsync()
        {
            await Init();
            await mTerminalShell.RunAsync();
            await Quit();
        }

        public void Halt()
        {
            mTerminalShell.Halt();
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
            await mTerminal.InitializeAsync(mEnv.BuiltIn);
            mTerminal.OnHaltInput += Halt;
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
            mTerminal.OnHaltInput -= Halt;
            await mTerminal.FinalizeAsync();
            await mEnv.FinalizeAsync();
            mEnv           = null;
            mFileSystem    = null;
            mTerminal      = null;
            mInterpreter   = null;
            mTerminalShell = null;
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
                        await mInterpreter.RunCommandAsync(mTerminalShell, c);
                    }
                }

                mIsUprofileExecuted = true;
            }

            if (mFileSystem.TryFindEntry(rc, out _))
            {
                await foreach (var c in mFileSystem.ReadLines(rc))
                {
                    await mInterpreter.RunCommandAsync(mTerminalShell, c);
                }
            }
        }
    }
}
