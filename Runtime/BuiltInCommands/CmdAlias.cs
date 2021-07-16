using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdAlias : UnishCommandBase
    {
        internal override bool IsBuiltIn => true;

        public override string[] Ops { get; } =
        {
            "alias",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "alias", "", "設定するエイリアス e.g. hoge=\"fuga piyo\""),
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "l", null, "定義済みのエイリアス一覧を表示します"),
        };

        public override string Usage(string op)
        {
            return "コマンドのエイリアスを作成します。";
        }

        protected override async UniTask Run(Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            if (options.ContainsKey("l"))
            {
                foreach (var a in Interpreter.Aliases)
                {
                    await IO.WriteLineAsync($"\"{a.Key}\" = \"{a.Value}\"");
                }
            }

            if (string.IsNullOrEmpty(args["alias"].S))
            {
                return;
            }


            var input      = args["alias"].S.Trim();
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

            if (Interpreter.Commands.ContainsKey(alias))
            {
                await IO.Err(new Exception($"The command {alias} already exists."));
                return;
            }

            var aliases = Interpreter.Aliases;
            if (string.IsNullOrWhiteSpace(command))
            {
                if (!aliases.ContainsKey(alias))
                {
                    await IO.Err(new Exception($"Alias {alias} does not exist."));
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
