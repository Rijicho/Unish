using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishRoot : IUnishRoot
    {
        public  IUnishProcess       Parent    => null;
        public  IUnishEnv           GlobalEnv { get; private set; }
        private IUnishEnv           mEnv;
        private IUnishIO            mIO;
        private IUnishInterpreter   mInterpreter;
        private IUnishDirectoryRoot mDirectory;
        private IUnishPresenter     mShell;
        private bool                mIsUprofileExecuted;

        // ----------------------------------
        // public methods
        // ----------------------------------
        public UnishRoot()
        {
            GlobalEnv    = new GlobalEnv();
            mEnv         = new ShellEnv();
            mIO          = new DefaultIO();
            mInterpreter = new DefaultInterpreter();
            mDirectory   = new DefaultDirectoryRoot();
            mShell       = new UnishCore(mEnv, mIO, mInterpreter, mDirectory, this);
        }

        public UnishRoot(IUnishEnv globalEnv, IUnishEnv shellEnv, IUnishIO io, IUnishInterpreter interpreter, IUnishDirectoryRoot directory)
        {
            GlobalEnv    = globalEnv;
            mEnv         = shellEnv;
            mIO          = io;
            mInterpreter = interpreter;
            mDirectory   = directory;
            mShell       = new UnishCore(shellEnv, io, interpreter, directory, this);
        }


        public async UniTask RunAsync()
        {
            await Init();
            await mShell.RunAsync();
            await Quit();
        }

        public void Halt()
        {
            mShell.Halt();
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
            await GlobalEnv.InitializeAsync();
            await mEnv.InitializeAsync();
            mIO.GlobalEnv = GlobalEnv;
            await mIO.InitializeAsync();
            mIO.OnHaltInput      += Halt;
            mDirectory.GlobalEnv =  GlobalEnv;
            await mDirectory.InitializeAsync();
            mInterpreter.GlobalEnv = GlobalEnv;
            await mInterpreter.InitializeAsync();

            if (GlobalEnv.TryGetValue(UnishBuiltInEnvKeys.HomePath, out var homePath))
            {
                mDirectory.TryChangeDirectory(homePath.S);
            }

            await OnPostInitAsync();
            await RunInitialScripts();
        }

        private async UniTask Quit()
        {
            await OnPreQuitAsync();
            await mInterpreter.FinalizeAsync();
            mInterpreter.GlobalEnv = null;
            await mDirectory.FinalizeAsync();
            mDirectory.GlobalEnv =  null;
            mIO.OnHaltInput      -= Halt;
            await mIO.FinalizeAsync();
            mIO.GlobalEnv = null;
            await mEnv.FinalizeAsync();
            await GlobalEnv.FinalizeAsync();
            GlobalEnv    = null;
            mEnv         = null;
            mDirectory   = null;
            mIO          = null;
            mInterpreter = null;
            mShell       = null;
            await OnPostQuitAsync();
        }


        private async UniTask RunInitialScripts()
        {
            var profile = GlobalEnv[UnishBuiltInEnvKeys.ProfilePath].S;
            var rc      = GlobalEnv[UnishBuiltInEnvKeys.RcPath].S;
            if (!mIsUprofileExecuted)
            {
                if (mDirectory.TryFindEntry(profile, out _))
                {
                    await foreach (var c in mDirectory.ReadLines(profile))
                    {
                        await mInterpreter.RunCommandAsync(mShell, c);
                    }
                }

                mIsUprofileExecuted = true;
            }

            if (mDirectory.TryFindEntry(rc, out _))
            {
                await foreach (var c in mDirectory.ReadLines(rc))
                {
                    await mInterpreter.RunCommandAsync(mShell, c);
                }
            }
        }
    }
}
