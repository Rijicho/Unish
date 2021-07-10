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

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args, Dictionary<string, UnishCommandArg> options)
        {
            var d = shell.CurrentDirectorySystem;
            if (d == null)
            {
                shell.SubmitError("Virtual directory system cannot be removed!");
                return default;
            }
            d.Delete(d.ConvertToHomeRelativePath(args["path"].s));
            return default;
        }
    }
}
