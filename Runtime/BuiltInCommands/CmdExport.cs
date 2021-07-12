using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdExport : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "export",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "varname", null, "環境変数にしたい変数名 or 変数代入式"),
        };

        protected override UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            var input = args["varname"].S;
            if (string.IsNullOrWhiteSpace(input))
            {
                return WriteUsage();
            }

            if (UnishCommandUtils.TryParseSetVarExpr(input, out var varname, out var value))
            {
                EnvVars.Set(varname, value);
                return default;
            }

            if (ShellVars.TryGetValue(input, out var val))
            {
                EnvVars[input] = val;
                return default;
            }

            return WriteUsage();
        }

        public override string Usage(string op)
        {
            return "シェル変数を環境変数にします。";
        }
    }
}
