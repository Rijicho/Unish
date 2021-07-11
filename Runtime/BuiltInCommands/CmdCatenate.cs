using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdCatenate : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "cat",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path1", null, "接続したいファイル１"),
            (UnishCommandArgType.String, "path2", null, "接続したいファイル２"),
        };

        public override string Usage(string op)
        {
            return "ファイルを連結して出力します。";
        }

        public override bool AllowTrailingNullParams => true;

        protected override async UniTask Run(string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var path1 = args["path1"].s;
            var path2 = args["path2"].s;
            var d     = Directory;

            if (string.IsNullOrEmpty(path1))
            {
                await WriteUsage();
                return;
            }

            var sb = new StringBuilder();
            try
            {
                sb.Append(Directory.Read(path1));
                if (!string.IsNullOrEmpty(path2))
                {
                    sb.Append(Directory.Read(path2));
                }

                await IO.WriteLineAsync(sb.ToString());
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException || e is InvalidOperationException)
            {
                await IO.WriteErrorAsync(new Exception(e.Message));
            }
        }
    }
}
