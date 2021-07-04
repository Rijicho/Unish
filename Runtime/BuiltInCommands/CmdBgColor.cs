using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdBgColor : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "bgcolor",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "color", "#000000AA", "背景色"),
        };

        public override string Usage(string op)
        {
            return "コンソールの背景色を変更します。";
        }

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            shell.View.BackgroundColor = shell.ColorParser.Parse(args["color"].s);
            return default;
        }
    }
}