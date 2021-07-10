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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "entry to delete"),
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.None, "r", null, "delete recursively"),
        };

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args, Dictionary<string, UnishCommandArg> options)
        {
            shell.Directory.Delete(args["path"].s, options.ContainsKey("r"));
            return default;
        }
    }
}
