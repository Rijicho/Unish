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

        protected override UniTask Run(string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            if (options.ContainsKey("r") && Directory.CurrentHome is IUnishRealFileSystem fileSystem)
            {
                return IO.WriteLineAsync(fileSystem.RealHomePath + Directory.Current.HomeRelativePath);
            }

            if (options.ContainsKey("a"))
            {
                return IO.WriteLineAsync(Directory.Current.FullPath);
            }

            if (Directory.Current.IsRoot)
            {
                return IO.WriteLineAsync(PathConstants.Root);
            }

            if (Directory.Current.IsHome)
            {
                return IO.WriteLineAsync(PathConstants.Home);
            }

            return IO.WriteLineAsync(PathConstants.Home + Directory.Current.HomeRelativePath);
        }

        public override string Usage(string op)
        {
            return "Show your current directory";
        }
    }
}
