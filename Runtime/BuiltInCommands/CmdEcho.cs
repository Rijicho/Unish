using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdEcho : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "echo",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "text", null, "表示するテキスト"),
        };

        public override bool RequiresPreParseArguments => false;

        protected override UniTask Run(string op, Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            return IO.WriteLineAsync(args[""].S);
        }

        public override string Usage(string op)
        {
            return "テキストを表示します。";
        }
    }
}
