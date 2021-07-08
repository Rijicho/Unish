using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdCatenate : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "cat",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path1", null, "接続したいファイル１"),
            (UnishCommandArgType.String, "path2", null, "接続したいファイル２"),
        };

        public override bool AllowTrailingNullParams => true;

        protected override async UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (!(shell.CurrentDirectorySystem is IUnishRealFileSystem fileSystem))
            {
                shell.SubmitError("current directory system is not a file system.");
                return;
            }

            var path1 = args["path1"].s;
            var path2 = args["path2"].s;

            if (string.IsNullOrEmpty(path1))
            {
                await shell.RunCommandAsync("man cat");
                return;
            }

            var sb = new StringBuilder();
            if (shell.CurrentDirectorySystem.TryFindEntry(path1, out var foundPath, out var hasChild) && !hasChild)
            {
                sb.Append(UnishIOUtility.ReadTextFile(fileSystem.RealHomePath + foundPath));
            }
            else
            {
                shell.SubmitError($"file {path1} not found.");
                return;
            }

            if (!string.IsNullOrEmpty(path2))
            {
                if (shell.CurrentDirectorySystem.TryFindEntry(path2, out foundPath, out hasChild) && !hasChild)
                {
                    sb.Append(UnishIOUtility.ReadTextFile(fileSystem.RealHomePath + foundPath));
                }
                else
                {
                    shell.SubmitError("$file {path2} not found.");
                    return;
                }
            }

            shell.SubmitText(sb.ToString());
        }
    }
}
