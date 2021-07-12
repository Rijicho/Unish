using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class Unish : IUnishRoot
    {
        public IUnishProcess       Parent      => null;
        public IUnishEnv           GlobalEnv   { get; private set; }
        public IUnishEnv           Env         { get; private set; }
        public IUnishIO            IO          { get; private set; }
        public IUnishInterpreter   Interpreter { get; private set; }
        public IUnishDirectoryRoot Directory   { get; private set; }

        private IUniShell mMainShell;
        private bool            mIsUprofileExecuted;

        // ----------------------------------
        // public methods
        // ----------------------------------

        public Unish(
            IUnishEnv globalEnv = default,
            IUnishEnv shellEnv = default,
            IUnishIO io = default,
            IUnishInterpreter interpreter = default,
            IUnishDirectoryRoot directory = default)
        {
            GlobalEnv   = globalEnv ?? new GlobalEnv();
            Env         = shellEnv ?? new ShellEnv();
            IO          = io ?? new DefaultIO();
            Interpreter = interpreter ?? new DefaultInterpreter();
            Directory   = directory ?? new DefaultDirectoryRoot();
            mMainShell  = new UnishCore(shellEnv, io, interpreter, directory, this);
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
            await GlobalEnv.InitializeAsync();
            await Env.InitializeAsync();
            IO.GlobalEnv = GlobalEnv;
            await IO.InitializeAsync();
            IO.OnHaltInput      += Halt;
            Directory.GlobalEnv =  GlobalEnv;
            await Directory.InitializeAsync();
            Interpreter.GlobalEnv = GlobalEnv;
            await Interpreter.InitializeAsync();

            if (GlobalEnv.TryGetValue(UnishBuiltInEnvKeys.HomePath, out var homePath))
            {
                Directory.TryChangeDirectory(homePath.S);
            }

            await OnPostInitAsync();
            await RunInitialScripts();
        }

        private async UniTask Quit()
        {
            await OnPreQuitAsync();
            await Interpreter.FinalizeAsync();
            Interpreter.GlobalEnv = null;
            await Directory.FinalizeAsync();
            Directory.GlobalEnv =  null;
            IO.OnHaltInput      -= Halt;
            await IO.FinalizeAsync();
            IO.GlobalEnv = null;
            await Env.FinalizeAsync();
            await GlobalEnv.FinalizeAsync();
            GlobalEnv   = null;
            Env         = null;
            Directory   = null;
            IO          = null;
            Interpreter = null;
            mMainShell  = null;
            await OnPostQuitAsync();
        }


        private async UniTask RunInitialScripts()
        {
            var profile = GlobalEnv[UnishBuiltInEnvKeys.ProfilePath].S;
            var rc      = GlobalEnv[UnishBuiltInEnvKeys.RcPath].S;
            if (!mIsUprofileExecuted)
            {
                if (Directory.TryFindEntry(profile, out _))
                {
                    await foreach (var c in Directory.ReadLines(profile))
                    {
                        await Interpreter.RunCommandAsync(mMainShell, c);
                    }
                }

                mIsUprofileExecuted = true;
            }

            if (Directory.TryFindEntry(rc, out _))
            {
                await foreach (var c in Directory.ReadLines(rc))
                {
                    await Interpreter.RunCommandAsync(mMainShell, c);
                }
            }
        }
    }
}
