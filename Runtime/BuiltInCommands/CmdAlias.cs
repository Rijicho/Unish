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
            (UnishCommandArgType.String, "alias", default, "設定するエイリアス"),
            (UnishCommandArgType.String, "command", "", "元のコマンド文（空欄でエイリアス削除）"),
        };

        public override string Usage(string op)
        {
            return "コマンドのエイリアスを作成します。";
        }

        public override bool RequiresPreParseArguments => false;

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var words = args[""].s.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));

            var alias = words.ElementAt(0);

            var command = words.Count() == 1 ? "" : args[""].s.Substring(alias.Length).Trim();

            if (string.IsNullOrWhiteSpace(alias))
            {
                shell.SubmitError("Invalid alias.");
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
                shell.SubmitSuccess($"Delete alias: {alias}");
            }
            else
            {
                if (aliases.ContainsKey(alias))
                {
                    shell.CommandRepository.Aliases[alias] = command;
                    shell.SubmitSuccess($"Update alias: {alias} = {command}");
                }
                else
                {
                    shell.CommandRepository.Aliases[alias] = command;
                    shell.SubmitSuccess($"Set alias: {alias} = {command}");
                }
            }

            shell.CommandRepository.SaveAlias();
            return default;
        }
    }
}