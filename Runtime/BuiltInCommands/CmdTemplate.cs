using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal abstract class CmdTemplate : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "", "", ""),
        };

        protected override UniTask Run(string op, Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            return UniTask.CompletedTask;
        }
    }
}
