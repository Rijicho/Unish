using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdChangeDirectory : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "cd",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "target path"),
        };

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (!shell.Directory.TryChangeCurrentDirectoryTo(args["path"].s)) shell.SubmitError("directory not found.");

            return default;
        }
    }
}