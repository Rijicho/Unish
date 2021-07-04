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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "pattern", null, "フィルタ"),
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.String, "s", "default", "ソートタイプ（default/name）"),
            (UnishCommandArgType.None, "d", "", "詳細表示"),
            (UnishCommandArgType.None, "r", "", "フィルタを正規表現とみなして検索"),
        };

        public override string Usage(string op)
        {
            return "コマンドリストを表示します。";
        }

        protected override async UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var filter = new Regex(options.ContainsKey("r") ? args["pattern"].s : $".*{args["pattern"].s}.*");
            var isFirst = true;

            IEnumerable<KeyValuePair<string, UnishCommandBase>> ls;

            if (options.ContainsKey("s") && options["s"].s == "name")
                ls = shell.CommandRepository.Map.OrderBy(x => x.Key);
            else
                ls = shell.CommandRepository.Map;

            foreach (var c in ls)
            {
                if (string.IsNullOrWhiteSpace(c.Key)) continue;
                if (c.Key.StartsWith("@")) continue;

                var m = filter.Match(c.Key);
                if (m.Success && m.Value == c.Key)
                {
                    if (options.ContainsKey("d"))
                    {
                        c.Value.SubmitUsage(c.Key, shell.SubmitTextIndented, isFirst);
                        isFirst = false;
                    }
                    else
                        shell.SubmitTextIndented(c.Key, "white", true);

                    await UniTask.Yield();
                }
            }
        }
    }
}