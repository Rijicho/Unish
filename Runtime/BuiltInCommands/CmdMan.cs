using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdMan : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "man",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "op", null, ""),
        };

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (shell.CommandRunner.Repository.Map.TryGetValue(args["op"].s, out var c))
            {
                c.SubmitUsage(args["op"].s, shell.SubmitTextIndented);
            }
            else if (shell.CommandRunner.Repository.Map.TryGetValue("@" + args["op"].s, out c))
            {
                c.SubmitUsage(args["op"].s, shell.SubmitTextIndented);
            }
            else
            {
                shell.SubmitError("Undefined Command.");
            }

            return UniTask.CompletedTask;
        }

        public override string Usage(string op)
        {
            return "指定したコマンドのマニュアルを表示します。";
        }
    }
}
