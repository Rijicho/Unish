using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdShellConfig : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "shpref",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "key", null, "変更したい項目 (bgcolor/prompt) "),
            (UnishCommandArgType.String, "value", null, "値"),
        };

        public override string Usage(string op)
        {
            return "Shellの環境設定をします。";
        }

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (string.IsNullOrEmpty(args["key"].s))
            {
                return shell.RunCommandAsync("man shpref");
            }

            switch (args["key"].s)
            {
                case "bgcolor":
                    {
                        shell.View.BackgroundColor = shell.ColorParser.Parse(args["value"].s ?? "#000000AA");
                        return default;
                    }
                case "prompt":
                    {
                        shell.Prompt = args["value"].s ?? "> ";
                        return default;
                    }
                default:
                    return shell.RunCommandAsync("man shpref");
            }
        }
    }
}
