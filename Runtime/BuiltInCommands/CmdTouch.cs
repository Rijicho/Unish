using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdTouch : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "touch",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "path", null, "file to create"),
        };

        protected override UniTask Run(string op, Dictionary<string, UnishVariable> args, Dictionary<string, UnishVariable> options)
        {
            Directory.Create(args["path"].S, false);
            return default;
        }

        public override string Usage(string op)
        {
            return "ファイルを作成します（タイムスタンプ系機能はありません）";
        }
    }
}
