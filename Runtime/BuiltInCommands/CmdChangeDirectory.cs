using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdChangeDirectory : UnishCommandBase
    {
        internal override bool IsBuiltIn => true;

        public override string[] Ops { get; } =
        {
            "cd",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "path", null, "target path"),
        };

        protected override async UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            var target = args["path"].S;
            if (Directory.TryFindEntry(target, out var entry))
            {
                Env.BuiltIn.Set(UnishBuiltInEnvKeys.WorkingDirectory, entry.Path);
            }
            else
            {
                await IO.WriteErrorAsync(new Exception($"Directory {target} does not exist."));
            }
        }
    }
}
