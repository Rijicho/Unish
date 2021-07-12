using System.Collections.Generic;
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
            foreach (var arg in args)
            {
                await IO.WriteLineAsync($"${arg.Key} = {arg.Value.S}");
            }
        }
    }
}
