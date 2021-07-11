using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class DefaultParser : IUnishParser
    {
        public UniTask InitializeAsync(IUnishEnv env)
        {
            return default;
        }

        public UniTask FinalizeAsync(IUnishEnv env)
        {
            return default;
        }

        public UnishParseResult Parse(string input, IUnishEnv env)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return default;
            }

            var noVariableInput = UnishCommandUtils.ParseVariables(input, env);
            var tokens          = UnishCommandUtils.SplitCommand(noVariableInput);

            var op      = tokens[0].token;
            var options = new List<string>();
            var args    = new List<string>();
            foreach (var t in tokens.Skip(1))
            {
                if (t.isOption)
                {
                    options.Add(t.token.TrimStart('-'));
                }
                else
                {
                    args.Add(t.token);
                }
            }

            return new UnishParseResult(op, args, options);
        }
    }
}
