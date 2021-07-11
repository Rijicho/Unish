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

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "file to create"),
        };

        protected override UniTask Run(string op, Dictionary<string, UnishCommandArg> args, Dictionary<string, UnishCommandArg> options)
        {
            Directory.Create(args["path"].s, false);
            return default;
        }

        public override string Usage(string op)
        {
            return "ファイルを作成します（タイムスタンプ系機能はありません）";
        }
    }
}
