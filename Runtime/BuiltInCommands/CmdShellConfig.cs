using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal sealed class CmdSet : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "set",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
        };
        protected override async UniTask Run(Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            foreach (var arg in args)
            {
                await IO.WriteLineAsync($"${arg.Key} = {arg.Value.S}");
            }
        }
    } 
    
    internal class CmdShellConfig : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "shpref",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "key", null, "変更したい項目 (prompt/bgcolor/width/height) "),
            (UnishVariableType.String, "value", null, "値"),
        };

        public override string Usage(string op)
        {
            return "Shellの環境設定をします。";
        }

        protected override UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            if (string.IsNullOrEmpty(args["key"].S))
            {
                return WriteUsage(IO);
            }

            switch (args["key"].S)
            {
                case "prompt":
                    {
                        Env.Set(BuiltInEnvKeys.Prompt, args["value"].S ?? "%d $ ");
                        return default;
                    }
                case "bgcolor":
                    {
                        Env.Set(BuiltInEnvKeys.BgColor, args["value"].S,
                            new UnishVariable(BuiltInEnvKeys.BgColor, DefaultColorParser.Instance.Parse("#000000cc")));
                        return default;
                    }
                case "width":
                    {
                        Env.Set(BuiltInEnvKeys.CharCountPerLine, args["value"].S, new UnishVariable(BuiltInEnvKeys.CharCountPerLine, 100));
                        return default;
                    }
                case "height":
                    {
                        Env.Set(BuiltInEnvKeys.LineCount, args["value"].S, new UnishVariable(BuiltInEnvKeys.LineCount, 24));
                        return default;
                    }
                default:
                    return WriteUsage(IO);
            }
        }
    }
}
