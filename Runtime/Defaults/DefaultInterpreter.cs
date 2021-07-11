using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class DefaultInterpreter : IUnishInterpreter
    {
        public IUnishCommandRepository     Repository { get; private set; }
        public IDictionary<string, string> Aliases    { get; private set; }

        // ----------------------------------
        // public methods
        // ----------------------------------
        public UniTask InitializeAsync(IUnishEnv env)
        {
            Repository = DefaultUnishCommandRepository.Instance;
            Aliases    = new Dictionary<string, string>();
            return Repository.InitializeAsync(env);
        }

        public async UniTask FinalizeAsync(IUnishEnv env)
        {
            await Repository.FinalizeAsync(env);
            Aliases    = null;
            Repository = null;
        }

        public async UniTask RunCommandAsync(IUnishPresenter shell, string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            cmd = cmd.TrimStart();

            // エイリアス解決
            foreach (var kv in Aliases)
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
            if (TryPreParseCommand(Repository, cmd,
                out var c, out var op, out var argsNotParsed))
            {
                try
                {
                    await c.Run(shell, op, argsNotParsed);
                }
                catch (Exception e)
                {
                    await shell.IO.WriteErrorAsync(e);
                }
            }
            // 失敗時の追加評価処理が定義されていれば実行
            else if (!await TryRunInvalidCommand(cmd))
            {
                await shell.IO.WriteErrorAsync(new Exception("Unknown Command. Enter 'h' to show help."));
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

        // ----------------------------------
        // private methods
        // ----------------------------------

        private bool TryPreParseCommand(IUnishCommandRepository repository, string cmd, out UnishCommandBase op, out string leading, out string trailing)
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

            return repository.Map.TryGetValue(leading, out op)
                   || repository.Map.TryGetValue("@" + leading, out op);
        }
    }
}
