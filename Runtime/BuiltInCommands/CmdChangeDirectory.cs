using System.Collections.Generic;
using System.Linq;
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
            var target = args["path"].s;

            if (shell.CurrentDirectorySystem == null)
            {
                if (target.StartsWith(PathConstants.CurrentRelativePrefix))
                    target = target.Substring(PathConstants.CurrentRelativePrefix.Length);

                shell.CurrentDirectorySystem = shell.DirectorySystems.FirstOrDefault(x => x.Home == target);

                if (shell.CurrentDirectorySystem == null)
                {
                    shell.SubmitError($"The directory system {target} does not exist.");
                }
                return default;
            }
            
            if (shell.CurrentDirectorySystem.IsRoot(target))
            {
                shell.CurrentDirectorySystem = null;
                return default;
            }
            
            if (!shell.CurrentDirectorySystem.TryChangeDirectoryTo(target)) shell.SubmitError("directory not found.");

            return default;
        }
    }
}