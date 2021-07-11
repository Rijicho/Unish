using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdMakeDirectory : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "mkdir",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "path", null, "directory to create"),
        };

        protected override UniTask Run(string op, Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            Directory.Create(args["path"].S, true);
            return default;
        }
    }
}
