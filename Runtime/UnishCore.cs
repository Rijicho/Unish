using System;
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
        public abstract IUnishIO            IO            { get; }
        public abstract IUnishCommandRunner CommandRunner { get; }
        public abstract IUnishDirectoryRoot Directory     { get; }
        public          string              Prompt        { get; set; } = "> ";

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
            await IO.InitializeAsync();
            await Directory.InitializeAsync();
            await CommandRunner.InitializeAsync();
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
                await this.RunCommandAsync(input);
            }
        }

        private async UniTask Quit()
        {
            await OnPreCloseAsync();
            await CommandRunner.FinalizeAsync();
            await Directory.FinalizeAsync();
            await IO.FinalizeAsync();
            await OnPostCloseAsync();
            mState = UnishState.None;
        }

        private string ParsedPrompt
        {
            get
            {
                if (!Prompt.Contains("%d"))
                {
                    return Prompt;
                }

                if (Directory.Current.IsRoot)
                {
                    return Prompt.Replace("%d", PathConstants.Root);
                }

                if (Directory.Current.IsHome)
                {
                    return Prompt.Replace("%d", PathConstants.Home);
                }

                return Prompt.Replace("%d", Directory.Current.Name);
            }
        }


        private async UniTask RunInitialScripts()
        {
            const string profile = "~/.uprofile";
            const string rc      = "~/.unishrc";
            try
            {
                if (!mIsUprofileExecuted)
                {
                    if (Directory.TryFindEntry(profile, out _))
                    {
                        await foreach (var c in Directory.ReadLines(profile))
                        {
                            await CommandRunner.RunCommandAsync(this, c);
                        }
                    }

                    mIsUprofileExecuted = true;
                }

                if (Directory.TryFindEntry(rc, out _))
                {
                    await foreach (var c in Directory.ReadLines(rc))
                    {
                        await this.RunCommandAsync(c);
                    }
                }
            }
            catch (Exception e)
            {
                this.SubmitError(e.Message);
                this.SubmitError(e.StackTrace);
            }
        }
    }
}
