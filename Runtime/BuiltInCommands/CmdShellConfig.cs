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
                        Env.Set(BuiltInEnvKeys.Prompt, args["value"].s ?? "%d $ ");
                        return default;
                    }
                case "bgcolor":
                    {
                        Env.Set(BuiltInEnvKeys.BgColor, args["value"].s,
                            new UnishCommandArg(BuiltInEnvKeys.BgColor, DefaultColorParser.Instance.Parse("#000000cc")));
                        return default;
                    }
                case "width":
                    {
                        Env.Set(BuiltInEnvKeys.CharCountPerLine, args["value"].s, new UnishCommandArg(BuiltInEnvKeys.CharCountPerLine, 100));
                        return default;
                    }
                case "height":
                    {
                        Env.Set(BuiltInEnvKeys.LineCount, args["value"].s, new UnishCommandArg(BuiltInEnvKeys.LineCount, 24));
                        return default;
                    }
                default:
                    return WriteUsage(IO);
            }
        }
    }
}
