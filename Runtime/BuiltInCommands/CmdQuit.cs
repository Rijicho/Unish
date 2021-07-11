using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdQuit : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "q",
        };

        public override string[] Aliases { get; } =
        {
            "quit",
            "exit",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        protected override UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            Unish.Stop();
            return default;
        }

        public override string Usage(string op)
        {
            return "コンソールを閉じます。";
        }
    }
}
