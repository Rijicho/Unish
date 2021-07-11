using System.Collections.Generic;
using System.Linq;

namespace RUtil.Debug.Shell
{
    public class UnishParseResult
    {
        public readonly string                Command;
        public readonly IReadOnlyList<string> Params;
        public readonly IReadOnlyList<string> Options;

        public UnishParseResult(string command, IEnumerable<string> args, IEnumerable<string> options)
        {
            Command = command;
            Params  = args.ToList();
            Options = options.ToList();
        }
    }
}
