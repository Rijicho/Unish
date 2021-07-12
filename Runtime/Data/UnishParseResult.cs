using System.Collections.Generic;
using System.Linq;

namespace RUtil.Debug.Shell
{
    public class UnishParseResult
    {
        public readonly string                                       Command;
        public readonly IReadOnlyList<(string Token, bool IsOption)> Tokens;

        public UnishParseResult(string command, IEnumerable<(string Token, bool IsOption)> tokens)
        {
            Command = command;
            Tokens  = tokens.ToList();
        }
    }
}
