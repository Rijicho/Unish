﻿using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal sealed class CmdSet : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "set",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        protected override async UniTask Run(Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            await IO.WriteLineAsync("Environment variables:");
            foreach (var arg in EnvVars.OrderBy(v => v.Key))
            {
                await IO.WriteLineAsync($"  ${arg.Key} = {arg.Value.S}");
            }

            await IO.WriteLineAsync();
            await IO.WriteLineAsync("Shell variables:");
            foreach (var arg in ShellVars.OrderBy(v => v.Key))
            {
                await IO.WriteLineAsync($"  ${arg.Key} = {arg.Value.S}");
            }
        }
    }
}
