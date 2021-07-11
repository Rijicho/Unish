using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class CmdListUpCommand : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "lc",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "pattern", "", "フィルタ"),
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.String, "s", "default", "ソートタイプ（default/name）"),
            (UnishVariableType.Unit, "d", "", "詳細表示"),
            (UnishVariableType.Unit, "r", "", "フィルタを正規表現とみなして検索"),
        };

        public override string Usage(string op)
        {
            return "コマンドリストを表示します。";
        }

        protected override async UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            var filter  = new Regex(options.ContainsKey("r") ? args["pattern"].S : $".*{args["pattern"].S}.*");
            var isFirst = true;

            IEnumerable<KeyValuePair<string, UnishCommandBase>> ls;

            if (options.ContainsKey("s") && options["s"].S == "name")
            {
                ls = Interpreter.Repository.Map.OrderBy(x => x.Key);
            }
            else
            {
                ls = Interpreter.Repository.Map;
            }

            foreach (var c in ls)
            {
                if (string.IsNullOrWhiteSpace(c.Key))
                {
                    continue;
                }

                if (c.Key.StartsWith("@"))
                {
                    continue;
                }

                var m = filter.Match(c.Key);
                if (m.Success && m.Value == c.Key)
                {
                    if (options.ContainsKey("d"))
                    {
                        await c.Value.WriteUsage(IO, c.Key, isFirst);
                        isFirst = false;
                    }
                    else
                    {
                        await IO.WriteLineAsync("| " + c.Key);
                    }

                    await UniTask.Yield();
                }
            }
        }
    }
}
