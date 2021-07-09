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

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            shell.SubmitTextIndented("==========================================", "orange");
            shell.SubmitTextIndented("[Help]", "orange");
            shell.SubmitTextIndented("Input command and press Enter key.", "orange");
            shell.SubmitNewLineIndented();
            shell.SubmitTextIndented("Important commands:", "orange");
            shell.SubmitTextIndented("----------------", "yellow");
            shell.CommandRepository.Map["lc"].SubmitUsage(shell.SubmitTextIndented, false, false);
            shell.SubmitTextIndented("----------------", "yellow");
            shell.CommandRepository.Map["man"].SubmitUsage(shell.SubmitTextIndented, false, false);
            shell.SubmitTextIndented("----------------", "yellow");
            shell.CommandRepository.Map["q"].SubmitUsage(shell.SubmitTextIndented, false, false);
            shell.SubmitTextIndented("----------------", "yellow");
            shell.SubmitNewLineIndented();
            shell.SubmitTextIndented("This view can be scrolled by Ctrl(Cmd)+Arrow keys.", "orange");
            shell.SubmitTextIndented("==========================================", "orange");
            return UniTask.CompletedTask;
        }

        public override string Usage(string op)
        {
            return "ヘルプを表示します。";
        }
    }
}
