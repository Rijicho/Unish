using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdRemove : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "rm",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "path", null, "entry to delete"),
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "r", null, "delete recursively"),
        };

        protected override UniTask Run(Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            Directory.Delete(args["path"].S, options.ContainsKey("r"));
            return default;
        }
    }
}
