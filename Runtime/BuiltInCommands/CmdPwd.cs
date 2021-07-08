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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } = { };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.None, "a", null, "show full path"),
            (UnishCommandArgType.None, "r", null, "show real full path (real file-system only)"),
        };

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (shell.CurrentDirectorySystem == null)
                shell.WriteLine(PathConstants.Root);
            else if (options.ContainsKey("r") && shell.CurrentDirectorySystem is IUnishRealFileSystem fileSystem)
                shell.WriteLine(fileSystem.RealHomePath + shell.CurrentDirectorySystem.Current);
            else if (options.ContainsKey("a"))
                shell.WriteLine(shell.CurrentDirectorySystem.GetCurrentFullPath());
            else
                shell.WriteLine(shell.CurrentDirectorySystem.Current);
            return default;
        }

        public override string Usage(string op)
        {
            return "Show the full path of your current directory";
        }
    }
}