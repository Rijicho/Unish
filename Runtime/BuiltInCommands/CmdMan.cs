using System;
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

        protected override UniTask Run(IUnishPresenter shell, string op,
            Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (shell.Interpreter.Repository.Map.TryGetValue(args["op"].s, out var c))
            {
                return c.WriteUsage(args["op"].s, shell.IO);
            }

            if (shell.Interpreter.Repository.Map.TryGetValue("@" + args["op"].s, out c))
            {
                return c.WriteUsage(args["op"].s, shell.IO);
            }

            return shell.IO.WriteErrorAsync(new Exception("Undefined Command."));
        }

        public override string Usage(string op)
        {
            return "指定したコマンドのマニュアルを表示します。";
        }
    }
}
