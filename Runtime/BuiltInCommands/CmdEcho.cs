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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "text", null, "表示するテキスト"),
        };

        public override bool RequiresPreParseArguments => false;

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            return shell.IO.WriteLineAsync(args[""].s);
        }

        public override string Usage(string op)
        {
            return "テキストを表示します。";
        }
    }
}
