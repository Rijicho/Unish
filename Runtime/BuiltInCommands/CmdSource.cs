using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdSource : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "source",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null,
                "Source-file's path to execute (CurrentDir: PersistentDataPath)"),
        };

        protected override async UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (!(shell.CurrentDirectorySystem is IUnishRealFileSystem fileSystem))
            {
                shell.SubmitError("Current directory system is not a file system");
                return;
            }

            if (shell.CurrentDirectorySystem.TryFindEntry(args["path"].s, out var foundPath, out var hasChild) && !hasChild)
            {
                var realPath = fileSystem.RealHomePath + foundPath;
                await foreach (var cmd in UnishIOUtility.ReadSourceFileLines(realPath))
                {
                    await shell.RunCommandAsync(cmd);
                }
            }
            else
            {
                shell.SubmitError($"The file \"{args["path"].s}\" is not found.");
            }
        }

        public override string Usage(string op)
        {
            return "Execute commands in a file placed under your PersistentDataPath.";
        }
    }
}
