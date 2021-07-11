using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class CmdHelp : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "h",
        };

        public override string[] Aliases { get; } =
        {
            "help",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        protected override async UniTask Run(string op, Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            await IO.WriteLineAsync("| " + "==========================================", "orange");
            await IO.WriteLineAsync("| " + "[Help]", "orange");
            await IO.WriteLineAsync("| " + "Input command and press Enter key.", "orange");
            await IO.WriteLineAsync("");
            await IO.WriteLineAsync("| " + "Important commands:", "orange");
            await IO.WriteLineAsync("| " + "----------------", "yellow");
            await Interpreter.Repository.Map["lc"].WriteUsage(IO, false, false);
            await IO.WriteLineAsync("| " + "----------------", "yellow");
            await Interpreter.Repository.Map["man"].WriteUsage(IO, false, false);
            await IO.WriteLineAsync("| " + "----------------", "yellow");
            await Interpreter.Repository.Map["q"].WriteUsage(IO, false, false);
            await IO.WriteLineAsync("| " + "----------------", "yellow");
            await IO.WriteLineAsync("");
            await IO.WriteLineAsync("| " + "This view can be scrolled by Ctrl(Cmd)+Arrow keys.", "orange");
            await IO.WriteLineAsync("| " + "==========================================", "orange");
        }

        public override string Usage(string op)
        {
            return "ヘルプを表示します。";
        }
    }
}
