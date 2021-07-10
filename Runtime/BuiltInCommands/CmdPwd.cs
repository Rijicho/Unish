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
                shell.IO.WriteLine(fileSystem.RealHomePath + shell.Directory.Current.HomeRelativePath);
                return default;
            }

            if (options.ContainsKey("a"))
            {
                shell.IO.WriteLine(shell.Directory.Current.FullPath);
                return default;
            }

            if (shell.Directory.Current.IsRoot)
            {
                shell.IO.WriteLine(PathConstants.Root);
                return default;
            }

            if (shell.Directory.Current.IsHome)
            {
                shell.IO.WriteLine(PathConstants.Home);
                return default;
            }

            shell.IO.WriteLine(PathConstants.HomeRelativePrefix + shell.Directory.Current.HomeRelativePath);

            return default;
        }

        public override string Usage(string op)
        {
            return "Show your current directory";
        }
    }
}
