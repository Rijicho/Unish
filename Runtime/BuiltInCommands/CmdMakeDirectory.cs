﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdMakeDirectory : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "mkdir",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "directory to create"),
        };

        protected override UniTask Run(string op, Dictionary<string, UnishCommandArg> args, Dictionary<string, UnishCommandArg> options)
        {
            Directory.Create(args["path"].s, true);
            return default;
        }
    }
}
