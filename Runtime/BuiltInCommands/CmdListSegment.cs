using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class CmdListSegment : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "ls",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishCommandArgType.Int, "R", "0", "list up recursively (DFS)"),
        };

        protected override async UniTask Run(string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var maxDepth = options.TryGetValue("R", out var value) ? value.i : 0;

            if (maxDepth > 0)
            {
                foreach (var (entry, depth) in Directory.GetCurrentChilds(maxDepth))
                {
                    await IO.WriteLineAsync("| " + new string(' ', depth * 2) + entry.Name);
                }
            }
            else
            {
                var childsEnumerable = Directory.GetCurrentChilds().Select(x => (x.entry.Name, x.entry.IsDirectory));
                var childs           = childsEnumerable.ToList();
                if (childs.Count == 0)
                {
                    return;
                }

                var maxCharCountPerChild = childs.Max(x => x.Name.Length) + 1;
                var maxCharCountPerLine  = Env.TryGet(BuiltInEnvKeys.CharCountPerLine, out int count) ? count : 100;
                var childNumPerLine      = maxCharCountPerLine / maxCharCountPerChild;
                var log                  = "";
                for (var i = 0; i < childs.Count; i++)
                {
                    var c = childs[i];
                    if (c.IsDirectory)
                    {
                        log += $"<color=cyan>{childs[i].Name.PadRight(maxCharCountPerChild)}</color>";
                    }
                    else
                    {
                        log += childs[i].Name.PadRight(maxCharCountPerChild);
                    }

                    if (i % childNumPerLine == childNumPerLine - 1)
                    {
                        await IO.WriteLineAsync(log);
                        log = "";
                    }
                }

                if (!string.IsNullOrEmpty(log))
                {
                    await IO.WriteLineAsync(log);
                }
            }
        }
    }
}
