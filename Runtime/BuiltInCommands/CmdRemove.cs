using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdRemove : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "rm",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "path", null, "entry to delete"),
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "r", null, "delete recursively"),
        };

        protected override UniTask Run(Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            var path = args["path"].S;
            if (Directory.TryFindEntry(path, out var entry))
            {
                if (entry.IsFileSystem)
                {
                    return IO.Err(new InvalidOperationException("Virtual filesystem cannot be deleted."));
                }

                if (entry.IsDirectory && Directory.GetChilds(entry.Path).Any() && !options.ContainsKey("r"))
                {
                    return IO.Err(new Exception($"The directory {path} has childs."));
                }
            }

            Directory.Delete(args["path"].S, options.ContainsKey("r"));
            return default;
        }
    }
}
