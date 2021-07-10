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

        public override string Usage(string op)
        {
            return "ファイルを連結して出力します。";
        }

        public override bool AllowTrailingNullParams => true;

        protected override async UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var path1  = args["path1"].s;
            var path2  = args["path2"].s;
            var d      = shell.CurrentDirectorySystem;

            if (string.IsNullOrEmpty(path1))
            {
                await shell.RunCommandAsync("man cat");
                return;
            }
            var hPath1 = d.ConvertToHomeRelativePath(path1);
            var sb     = new StringBuilder();
            if (d.TryFindEntry(hPath1, out var hasChild) && !hasChild)
            {
                sb.Append(d.Read(hPath1));
            }
            else
            {
                shell.SubmitError($"file \"{hPath1}\" not found.");
                return;
            }

            if (!string.IsNullOrEmpty(path2))
            {
                var hPath2 = d.ConvertToHomeRelativePath(path2);
                if (d.TryFindEntry(hPath2, out hasChild) && !hasChild)
                {
                    sb.Append(d.Read(hPath2));
                }
                else
                {
                    shell.SubmitError($"file \"{hPath2}\" not found.");
                    return;
                }
            }

            shell.SubmitText(sb.ToString());
        }
    }
}
