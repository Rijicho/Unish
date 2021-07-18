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

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "a", null, "include entries that begin with '.'"),
            (UnishVariableType.Int, "R", "0", "list up recursively (DFS)"),
        };

        protected override async UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            var maxDepth = options.TryGetValue("R", out var value) ? value.I : 0;
            var cd       = Env.BuiltIn[UnishBuiltInEnvKeys.WorkingDirectory].S;

            if (maxDepth > 0)
            {
                foreach (var (entry, depth) in Directory.GetChilds(cd, maxDepth))
                {
                    if (options.ContainsKey("a") || entry.Name[0] != '.')
                    {
                        await IO.WriteLineAsync("| " + new string(' ', depth * 2) + entry.Name);
                    }
                }
            }
            else
            {
                var childsEnumerable = Directory.GetChilds(cd).Select(x => (x.Entry.Name, x.Entry.IsDirectory, x.Entry.IsFileSystem));
                var childs           = childsEnumerable.ToList();
                if (childs.Count == 0)
                {
                    return;
                }

                var maxCharCountPerChild = childs.Max(x => x.Name.Length) + 1;
                var maxCharCountPerLine  = Env.BuiltIn.Get(UnishBuiltInEnvKeys.CharCountPerLine, 100);
                var childNumPerLine      = maxCharCountPerLine / maxCharCountPerChild;
                var log                  = "";
                for (var i = 0; i < childs.Count; i++)
                {
                    var c = childs[i];
                    if (c.Name[0] == '.' && !options.ContainsKey("a"))
                    {
                        continue;
                    }
                    if (c.IsDirectory || c.IsFileSystem)
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
