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
            (UnishCommandArgType.String, "key", null, "変更したい項目 (prompt/bgcolor/width/height) "),
            (UnishCommandArgType.String, "value", null, "値"),
        };

        public override string Usage(string op)
        {
            return "Shellの環境設定をします。";
        }

        protected override UniTask Run(string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (string.IsNullOrEmpty(args["key"].s))
            {
                return WriteUsage(IO);
            }

            switch (args["key"].s)
            {
                case "prompt":
                    {
                        Env[UnishBuiltInEnvKeys.Prompt] = args["value"].s ?? "> ";
                        return default;
                    }
                case "bgcolor":
                    {
                        Env[UnishBuiltInEnvKeys.BgColor] = args["value"].s ?? "#000000cc";
                        return default;
                    }
                case "width":
                    {
                        Env[UnishBuiltInEnvKeys.CharCountPerLine] = args["value"].s ?? "100";
                        return default;
                    }
                case "height":
                    {
                        Env[UnishBuiltInEnvKeys.LineCount] = args["value"].s ?? "24";
                        return default;
                    }
                default:
                    return WriteUsage(IO);
            }
        }
    }
}
