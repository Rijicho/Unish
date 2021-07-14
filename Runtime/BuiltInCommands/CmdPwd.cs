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
            var wd = Env.BuiltIn[UnishBuiltInEnvKeys.WorkingDirectory].S;
            if (options.ContainsKey("r") && Directory.CurrentHome is IUnishRealFileSystem fileSystem)
            {
                return IO.WriteLineAsync(fileSystem.RealRootPath + wd.Substring(Directory.CurrentHome.RootPath.Length));
            }

            if (options.ContainsKey("a"))
            {
                return IO.WriteLineAsync(wd);
            }

            if (wd == UnishPathConstants.Root)
            {
                return IO.WriteLineAsync(UnishPathConstants.Root);
            }

            if (Directory.CurrentHome == default)
            {
                return IO.WriteLineAsync(wd);
            }

            return IO.WriteLineAsync(UnishPathConstants.Home + wd.Substring(Directory.CurrentHome.RootPath.Length));
        }

        public override string Usage(string op)
        {
            return "Show your current directory";
        }
    }
}
