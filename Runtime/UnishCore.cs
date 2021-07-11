using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public abstract class UnishCore : IUnishPresenter
    {
        // ----------------------------------
        // non-serialized fields
        // ----------------------------------

        private static bool       mIsUprofileExecuted;
        private        UnishState mState;

        // ----------------------------------
        // properties
        // ----------------------------------
        public abstract IUnishEnv           Env         { get; }
        public abstract IUnishIO            IO          { get; }
        public abstract IUnishInterpreter   Interpreter { get; }
        public abstract IUnishDirectoryRoot Directory   { get; }

        // ----------------------------------
        // public methods
        // ----------------------------------

        public async UniTask RunAsync()
        {
            await Init();
            await Loop();
            await Quit();
        }

        public void Halt()
        {
            mState = UnishState.Quit;
        }


        // ----------------------------------
        // protected methods
        // ----------------------------------

        protected virtual UniTask OnPreOpenAsync()
        {
            return default;
        }

        protected virtual UniTask OnPostOpenAsync()
        {
            return default;
        }

        protected virtual UniTask OnPreCloseAsync()
        {
            return default;
        }

        protected virtual UniTask OnPostCloseAsync()
        {
            return default;
        }

        // ----------------------------------
        // private methods
        // ----------------------------------


        private async UniTask Init()
        {
            mState = UnishState.Init;

            await OnPreOpenAsync();
            await Env.InitializeAsync(null);
            await IO.InitializeAsync(Env);
            IO.OnHaltInput += Halt;
            await Directory.InitializeAsync(Env);
            await Interpreter.InitializeAsync(Env);

            if (Env.TryGetValue(UnishBuiltInEnvKeys.HomePath, out var homePath))
            {
                Directory.TryChangeDirectory(homePath);
            }

            await OnPostOpenAsync();
        }

        private async UniTask Loop()
        {
            await RunInitialScripts();
            while (mState != UnishState.Quit)
            {
                mState = UnishState.Wait;
                await IO.WriteAsync(ParsedPrompt);
                var input = await IO.ReadAsync();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                mState = UnishState.Run;
                await Interpreter.RunCommandAsync(this, input);
            }
        }

        private async UniTask Quit()
        {
            await OnPreCloseAsync();
            await Interpreter.FinalizeAsync(Env);
            await Directory.FinalizeAsync(Env);
            IO.OnHaltInput -= Halt;
            await IO.FinalizeAsync(Env);
            await Env.FinalizeAsync(Env);
            await OnPostCloseAsync();
            mState = UnishState.None;
        }

        private string ParsedPrompt
        {
            get
            {
                var prompt = Env[UnishBuiltInEnvKeys.Prompt];
                if (!prompt.Contains("%d"))
                {
                    return prompt;
                }

                if (Directory.Current.IsRoot)
                {
                    return prompt.Replace("%d", PathConstants.Root);
                }

                if (Directory.Current.IsHome)
                {
                    return prompt.Replace("%d", PathConstants.Home);
                }

                return prompt.Replace("%d", Directory.Current.Name);
            }
        }


        private async UniTask RunInitialScripts()
        {
            var profile = Env[UnishBuiltInEnvKeys.ProfilePath];
            var rc      = Env[UnishBuiltInEnvKeys.RcPath];
            if (!mIsUprofileExecuted)
            {
                if (Directory.TryFindEntry(profile, out _))
                {
                    await foreach (var c in Directory.ReadLines(profile))
                    {
                        await Interpreter.RunCommandAsync(this, c);
                    }
                }

                mIsUprofileExecuted = true;
            }

            if (Directory.TryFindEntry(rc, out _))
            {
                await foreach (var c in Directory.ReadLines(rc))
                {
                    await Interpreter.RunCommandAsync(this, c);
                }
            }
        }
    }
}
