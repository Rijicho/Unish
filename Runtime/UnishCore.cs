using System;
using System.Collections.Generic;
using System.IO;
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
        public          UnishState                         State                  { get; private set; }
        public abstract IUnishView                         View                   { get; }
        public abstract IUnishCommandRepository            CommandRepository      { get; }
        public abstract IUnishCommandRunner                CommandRunner          { get; }
        public abstract IUnishColorParser                  ColorParser            { get; }
        public abstract IUnishTimeProvider                 TimeProvider           { get; }
        public abstract IUnishRcRepository                 RcRepository           { get; }
        public abstract IEnumerable<IUnishDirectorySystem> DirectorySystems       { get; }
        public          IUnishDirectorySystem              CurrentDirectorySystem { get; set; }
        public          string                             Prompt                 { get; set; } = "> ";

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

        protected virtual UniTask<bool> TryRunInvalidCommand(string cmd)
        {
            return UniTask.FromResult(false);
        }

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
            await View.InitializeAsync();
            CommandRepository.Initialize();
            await OnPostOpenAsync();
            await RunRcAndProfile();
        }

        private async UniTask Loop()
        {
            while (State != UnishState.Quit)
            {
                State = UnishState.Wait;
                View.Write(ParsedPrompt);
                var input = await View.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                State = UnishState.Run;
                await this.RunCommandAsync(input);
            }

            UnityEngine.Debug.Log("loopend");
        }

        private async UniTask Quit()
        {
            await OnPreCloseAsync();
            await View.DestroyAsync();
            await OnPostCloseAsync();
            State = UnishState.None;
        }

        private string ParsedPrompt =>
            Prompt.Replace("%d", CurrentDirectorySystem == null ? PathConstants.Root
                : string.IsNullOrEmpty(CurrentDirectorySystem.Current) ? PathConstants.Home
                : Path.GetFileName(CurrentDirectorySystem.Current));



        private async UniTask RunRcAndProfile()
        {
            try
            {
                if (!mIsUprofileExecuted)
                {
                    await foreach (var c in RcRepository.ReadUProfile())
                    {
                        await this.RunCommandAsync(c);
                    }

                    mIsUprofileExecuted = true;
                }

                await foreach (var c in RcRepository.ReadUnishRc())
                {
                    await this.RunCommandAsync(c);
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
