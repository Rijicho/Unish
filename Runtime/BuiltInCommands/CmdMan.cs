﻿using System;
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

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "op", null, ""),
        };

        protected override UniTask Run(
            Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            if (Interpreter.Commands.TryGetValue(args["op"].S, out var c))
            {
                return c.WriteUsage(IO, args["op"].S);
            }

            if (Interpreter.Commands.TryGetValue("@" + args["op"].S, out c))
            {
                return c.WriteUsage(IO, args["op"].S);
            }

            return IO.Err(new Exception("Undefined Command."));
        }

        public override string Usage(string op)
        {
            return "指定したコマンドのマニュアルを表示します。";
        }
    }
}
