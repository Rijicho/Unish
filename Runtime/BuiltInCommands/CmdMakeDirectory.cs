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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "directory to create"),
        };

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args, Dictionary<string, UnishCommandArg> options)
        {
            var d = shell.CurrentDirectorySystem;
            if (d == null)
            {
                shell.SubmitError("Virtual directory system cannot be created with this command!");
                return default;
            }
            d.Create(d.ConvertToHomeRelativePath(args["path"].s), true);
            return default;
        }
    }
}
