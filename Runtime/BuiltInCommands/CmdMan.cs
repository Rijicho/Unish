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

        protected override UniTask Run(string op,
            Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (Interpreter.Repository.Map.TryGetValue(args["op"].s, out var c))
            {
                return c.WriteUsage(IO, args["op"].s);
            }

            if (Interpreter.Repository.Map.TryGetValue("@" + args["op"].s, out c))
            {
                return c.WriteUsage(IO, args["op"].s);
            }

            return IO.WriteErrorAsync(new Exception("Undefined Command."));
        }

        public override string Usage(string op)
        {
            return "指定したコマンドのマニュアルを表示します。";
        }
    }
}
