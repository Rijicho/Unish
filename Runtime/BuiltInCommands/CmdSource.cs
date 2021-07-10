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
            (UnishCommandArgType.String, "path", null, "Source-file's path to execute"),
        };

        protected override async UniTask Run(IUnishPresenter shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var path = args["path"].s;
            await foreach (var line in shell.Directory.ReadLines(path))
            {
                var cmd = line.Trim();
                if (string.IsNullOrWhiteSpace(cmd))
                {
                    continue;
                }

                if (cmd.StartsWith("#"))
                {
                    continue;
                }

                await shell.RunCommandAsync(cmd);
            }
        }

        public override string Usage(string op)
        {
            return "Execute commands in given file.";
        }
    }
}
