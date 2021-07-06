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

        protected override async UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (UnishIOUtility.Exists(args["path"].s, out var realPath))
            {
                await foreach (var cmd in UnishIOUtility.ReadSourceFile(realPath))
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