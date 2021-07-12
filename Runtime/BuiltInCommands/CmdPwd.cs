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

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "a", null, "show full path"),
            (UnishVariableType.Unit, "r", null, "show real full path (real file-system only)"),
        };

        protected override UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
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
                return IO.WriteLineAsync(UnishPathConstants.Root);
            }

            if (Directory.Current.IsHome)
            {
                return IO.WriteLineAsync(UnishPathConstants.Home);
            }

            return IO.WriteLineAsync(UnishPathConstants.Home + Directory.Current.HomeRelativePath);
        }

        public override string Usage(string op)
        {
            return "Show your current directory";
        }
    }
}
