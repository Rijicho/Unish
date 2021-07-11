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

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "path", null, "Source-file's path to execute"),
        };

        protected override async UniTask Run(string op, Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            var path = args["path"].S;
            await foreach (var line in Directory.ReadLines(path))
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

                await RunNewCommandAsync(cmd);
            }
        }

        public override string Usage(string op)
        {
            return "Execute commands in given file.";
        }
    }
}
