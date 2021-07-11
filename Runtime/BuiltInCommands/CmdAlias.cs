using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdAlias : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "alias",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "alias", "", "設定するエイリアス e.g. hoge=\"fuga piyo\""),
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.None, "l", null, "定義済みのエイリアス一覧を表示します"),
        };

        public override string Usage(string op)
        {
            return "コマンドのエイリアスを作成します。";
        }

        protected override async UniTask Run(string op, Dictionary<string, UnishCommandArg> args, Dictionary<string, UnishCommandArg> options)
        {
            if (options.ContainsKey("l"))
            {
                foreach (var a in Interpreter.Aliases)
                {
                    await IO.WriteLineAsync($"\"{a.Key}\" = \"{a.Value}\"");
                }
            }

            if (string.IsNullOrEmpty(args["alias"].s))
            {
                return;
            }


            var input      = args["alias"].s.Trim();
            var firstEqual = input.IndexOf('=');
            if (firstEqual < 0)
            {
                await WriteUsage(IO);
                return;
            }


            var alias   = input.Substring(0, firstEqual).Trim();
            var command = input.Substring(firstEqual + 1).Trim();
            if ((command[0] == '"' && command[command.Length - 1] == '"')
                || (command[0] == '\'' && command[command.Length - 1] == '\''))
            {
                command = command.Substring(1, command.Length - 2);
            }

            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(alias))
            {
                await WriteUsage(IO);
                return;
            }

            if (Interpreter.Repository.Commands.Count(x => x.Ops.Contains(alias)) > 0)
            {
                await IO.WriteErrorAsync(new Exception($"The command {alias} already exists."));
                return;
            }

            var aliases = Interpreter.Aliases;
            if (string.IsNullOrWhiteSpace(command))
            {
                if (!aliases.ContainsKey(alias))
                {
                    await IO.WriteErrorAsync(new Exception($"Alias {alias} does not exist."));
                    return;
                }

                aliases.Remove(alias);
            }
            else
            {
                Interpreter.Aliases[alias] = command;
            }
        }
    }
}
