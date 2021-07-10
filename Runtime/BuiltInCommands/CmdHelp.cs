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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        protected override async UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            await shell.IO.WriteLineAsync("| "+"==========================================", "orange");
            await shell.IO.WriteLineAsync("| "+"[Help]", "orange");
            await shell.IO.WriteLineAsync("| "+"Input command and press Enter key.", "orange");
            await shell.IO.WriteLineAsync("");
            await shell.IO.WriteLineAsync("| "+"Important commands:", "orange");
            await shell.IO.WriteLineAsync("| "+"----------------", "yellow");
            await shell.Interpreter.Repository.Map["lc"].SubmitUsage(shell.IO, false, false);
            await shell.IO.WriteLineAsync("| "+"----------------", "yellow");
            await shell.Interpreter.Repository.Map["man"].SubmitUsage(shell.IO, false, false);
            await shell.IO.WriteLineAsync("| "+"----------------", "yellow");
            await shell.Interpreter.Repository.Map["q"].SubmitUsage(shell.IO, false, false);
            await shell.IO.WriteLineAsync("| "+"----------------", "yellow");
            await shell.IO.WriteLineAsync("");
            await shell.IO.WriteLineAsync("| "+"This view can be scrolled by Ctrl(Cmd)+Arrow keys.", "orange");
            await shell.IO.WriteLineAsync("| "+"==========================================", "orange");
        }

        public override string Usage(string op)
        {
            return "ヘルプを表示します。";
        }
    }
}
