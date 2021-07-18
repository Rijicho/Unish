using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdGrep : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "grep",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "pattern", "", "match pattern"),
            (UnishVariableType.String, "file", null, "source file"),
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "i", null, "ignore cases"),
            (UnishVariableType.Unit, "E", null, "use regular expressions"),
        };

        protected override async UniTask Run(Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            var source  = args["file"].S;
            var pattern = args["pattern"].S;
            if (string.IsNullOrEmpty(pattern))
            {
                await WriteUsage();
                return;
            }

            var sourceEnumerable = string.IsNullOrEmpty(source)
                ? IO.In(false)
                : Directory.ReadLines(source);
            await foreach (var line in sourceEnumerable)
            {
                var match = options.ContainsKey("E") ? Regex.IsMatch(line, pattern)
                    : options.ContainsKey("i") ? line.ToLower().Contains(pattern.ToLower())
                    : line.Contains(pattern);

                if (match)
                {
                    await IO.WriteLineAsync(line);
                }
            }
        }
    }
}
