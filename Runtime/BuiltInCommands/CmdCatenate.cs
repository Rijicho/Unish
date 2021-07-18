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

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "paths", null, "接続したいファイル"),
        };

        public override string Usage(string op)
        {
            return "ファイルを連結して出力します。";
        }

        protected override async UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            var sb = new StringBuilder();
            if (args["#"].I == 0)
            {
                await foreach (var input in IO.In(false))
                {
                    sb.AppendLine(input);
                }
            }

            for (var i = 1; i <= args["#"].I; i++)
            {
                var path = args[$"{i}"].S;
                try
                {
                    sb.Append(Directory.Read(path));
                }
                catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException || e is InvalidOperationException)
                {
                    await IO.Err(new Exception(e.Message));
                }
            }

            if (sb[sb.Length - 1] != '\n')
            {
                sb.AppendLine();
            }
            await IO.Out(sb.ToString());
        }
    }
}
