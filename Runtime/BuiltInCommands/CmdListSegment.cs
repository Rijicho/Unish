using System.Collections.Generic;
using System.IO;
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

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var maxDepth = options.TryGetValue("R", out var value) ? value.i : 0;
            if (maxDepth != 0)
            {
                foreach (var (filePath, depth, hasChild) in shell.Directory.GetCurrentChilds(maxDepth))
                    shell.SubmitTextIndented(new string(' ', depth * 2) + Path.GetFileName(filePath));
            }
            else
            {
                var childs = shell.Directory.GetCurrentChilds()
                    .Select(child => (filename: Path.GetFileName(child.path), isDirectory: child.hasChild))
                    .ToList();
                var maxCharCountPerChild = childs.Max(x => x.filename.Length) + 1;
                var maxCharCountPerLine = shell.View.HorizontalCharCount;
                var childNumPerLine = maxCharCountPerLine / maxCharCountPerChild;
                var log = "";
                for (var i = 0; i < childs.Count; i++)
                {
                    var c = childs[i];
                    if (c.isDirectory)
                        log += $"<color=cyan>{childs[i].filename.PadRight(maxCharCountPerChild)}</color>";
                    else
                        log += childs[i].filename.PadRight(maxCharCountPerChild);
                    if (i % childNumPerLine == childNumPerLine - 1)
                    {
                        shell.SubmitText(log);
                        log = "";
                    }
                }

                shell.SubmitText(log);
            }

            return default;
        }
    }
}