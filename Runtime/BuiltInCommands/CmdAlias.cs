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
            (UnishCommandArgType.String, "alias", default, "設定するエイリアス e.g. hoge=\"fuga piyo\""),
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.None, "l", null, "定義済みのエイリアス一覧を表示します"),
        };

        public override string Usage(string op)
        {
            return "コマンドのエイリアスを作成します。";
        }

        public override bool AllowTrailingNullParams => true;

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (options.ContainsKey("l"))
            {
                foreach (var a in shell.CommandRepository.Aliases)
                    shell.SubmitTextIndented($"\"{a.Key}\" = \"{a.Value}\"");

                return default;
            }


            var input = args["alias"].s.Trim();
            var firstEqual = input.IndexOf('=');
            if (firstEqual < 0)
            {
                SubmitUsage(shell.SubmitTextIndented);
                return default;
            }
            

            var alias = input.Substring(0, firstEqual).Trim();
            var command = input.Substring(firstEqual+1).Trim();
            if (command[0] == '"' && command[command.Length - 1] == '"')
            {
                command = command.Substring(1, command.Length - 2);
            }
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(alias))
            {
                SubmitUsage(shell.SubmitTextIndented);
                return default;
            }

            if (shell.CommandRepository.Commands.Count(x => x.Ops.Contains(alias)) > 0)
            {
                shell.SubmitError($"The command {alias} already exists.");
                return default;
            }

            var aliases = shell.CommandRepository.Aliases;
            if (string.IsNullOrWhiteSpace(command))
            {
                if (!aliases.ContainsKey(alias))
                {
                    shell.SubmitError($"Alias {alias} does not exist.");
                    return default;
                }

                aliases.Remove(alias);
            }
            else
                shell.CommandRepository.Aliases[alias] = command;

            return default;
        }
    }
}