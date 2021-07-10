using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdPwd : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "pwd",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.None, "a", null, "show full path"),
            (UnishCommandArgType.None, "r", null, "show real full path (real file-system only)"),
        };

        protected override UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (options.ContainsKey("r") && shell.Directory.CurrentHome is IUnishRealFileSystem fileSystem)
            {
                return shell.IO.WriteLineAsync(fileSystem.RealHomePath + shell.Directory.Current.HomeRelativePath);
            }

            if (options.ContainsKey("a"))
            {
                return shell.IO.WriteLineAsync(shell.Directory.Current.FullPath);
            }

            if (shell.Directory.Current.IsRoot)
            {
                return shell.IO.WriteLineAsync(PathConstants.Root);
            }

            if (shell.Directory.Current.IsHome)
            {
                return shell.IO.WriteLineAsync(PathConstants.Home);
            }

            return shell.IO.WriteLineAsync(PathConstants.Home + shell.Directory.Current.HomeRelativePath);
        }

        public override string Usage(string op)
        {
            return "Show your current directory";
        }
    }
}
