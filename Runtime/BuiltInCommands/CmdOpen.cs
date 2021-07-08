using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    internal class CmdOpen : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "open",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "URL or file path to open (CurrentDir: PersistentDataPath)"),
        };

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var path = args["path"].s;

            if (UnishIOUtility.IsValidUrlPath(path))
            {
                Application.OpenURL(path);
                return default;
            }

            if (shell.CurrentDirectorySystem is IUnishRealFileSystem fileSystem
                && shell.CurrentDirectorySystem.TryFindEntry(path, out var foundPath, out _))
            {
                Application.OpenURL(fileSystem.RealHomePath + foundPath);
                return default;
            }

            if (UnishIOUtility.IsValidUrlPath("https://" + path))
            {
                Application.OpenURL("https://" + path);
                return default;
            }

            return default;
        }

        public override string Usage(string op)
        {
            return "指定したURLまたはファイルを開きます。\nファイルは絶対パスか、PersistentDataPathをカレントディレクトリとする相対パスで指定してください。";
        }
    }
}
