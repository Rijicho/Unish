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

        public async UniTask RunCommandAsync(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            cmd = cmd.TrimStart();

            // エイリアス解決
            foreach (var kv in CommandRepository.Aliases)
            {
                if (cmd.TrimEnd() == kv.Key)
                {
                    cmd = kv.Value;
                    break;
                }

                if (cmd.StartsWith(kv.Key + " "))
                {
                    cmd = kv.Value + cmd.Substring(kv.Key.Length);
                    break;
                }
            }

            // オペランドのみパースし、コマンドが存在すれば実行
            if (TryPreParseCommand(cmd, out var c, out var op, out var argsNotParsed))
            {
                try
                {
                    await c.Run(this, op, argsNotParsed, this.SubmitTextIndented, this.SubmitError);
                }
                catch (Exception e)
                {
                    this.SubmitError(e.Message ?? "");
                    this.SubmitTextIndented(e.StackTrace, "#ff7777");
                }
            }
            // 失敗時の追加評価処理が定義されていれば実行
            else if (!await TryRunInvalidCommand(cmd))
            {
                this.SubmitError("Unknown Command/Expr. Enter 'h' to show help.");
            }

            await UniTask.Yield();
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
                await RunCommandAsync(input);
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


        private bool TryPreParseCommand(string cmd, out UnishCommandBase op, out string leading, out string trailing)
        {
            leading  = cmd;
            trailing = "";
            for (var i = 0; i < cmd.Length; i++)
            {
                if (cmd[i] == ' ')
                {
                    leading  = cmd.Substring(0, i);
                    trailing = cmd.Substring(i + 1);
                    break;
                }
            }

            return CommandRepository.Map.TryGetValue(leading, out op)
                   || CommandRepository.Map.TryGetValue("@" + leading, out op);
        }

        private async UniTask RunRcAndProfile()
        {
            try
            {
                if (!mIsUprofileExecuted)
                {
                    await foreach (var c in RcRepository.ReadUProfile())
                    {
                        await RunCommandAsync(c);
                    }

                    mIsUprofileExecuted = true;
                }

                await foreach (var c in RcRepository.ReadUnishRc())
                {
                    await RunCommandAsync(c);
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
