using System;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public abstract class UnishCore : IUnishPresenter
    {
        // ----------------------------------
        // non-serialized fields
        // ----------------------------------

        private static bool mIsUprofileExecuted;

        // ----------------------------------
        // properties
        // ----------------------------------
        public          UnishState              State             { get; private set; }
        public abstract IUnishIO                IO                { get; }
        public abstract IUnishCommandRepository CommandRepository { get; }
        public abstract IUnishCommandRunner     CommandRunner     { get; }
        public abstract IUnishColorParser       ColorParser       { get; }
        public abstract IUnishDirectoryRoot     Directory         { get; }
        public          string                  Prompt            { get; set; } = "> ";

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
            State = UnishState.Quit;
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
            State = UnishState.Init;

            await OnPreOpenAsync();
            await IO.InitializeAsync();
            await Directory.InitializeAsync();
            await CommandRepository.InitializeAsync();
            await CommandRunner.InitializeAsync();
            await OnPostOpenAsync();
            await RunRcAndProfile();
        }

        private async UniTask Loop()
        {
            while (State != UnishState.Quit)
            {
                State = UnishState.Wait;
                await IO.WriteAsync(ParsedPrompt);
                var input = await IO.ReadAsync();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                State = UnishState.Run;
                await this.RunCommandAsync(input);
            }
        }

        private async UniTask Quit()
        {
            await OnPreCloseAsync();
            await CommandRunner.FinalizeAsync();
            await CommandRepository.FinalizeAsync();
            await Directory.FinalizeAsync();
            await IO.FinalizeAsync();
            await OnPostCloseAsync();
            State = UnishState.None;
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


        private async UniTask RunRcAndProfile()
        {
            try
            {
                if (!mIsUprofileExecuted)
                {
                    if (Directory.TryFindEntry("~/.uprofile", out _))
                    {
                        await foreach (var c in Directory.ReadLines("~/.uprofile"))
                        {
                            await CommandRunner.RunCommandAsync(this, c);
                        }
                    }

                    mIsUprofileExecuted = true;
                }

                if (Directory.TryFindEntry("~/.unishrc", out _))
                {
                    await foreach (var c in Directory.ReadLines("~/.unishrc"))
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
