using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    internal class CmdArgTest : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "argtest",
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishVariableType.String, "string", "hoge", "表示するテキスト"),
            (UnishVariableType.Bool, "bool", "false", "表示するテキスト"),
            (UnishVariableType.Int, "int", "57", "表示するテキスト"),
            (UnishVariableType.Float, "float", "2.71828", "表示するテキスト"),
            (UnishVariableType.Vector2, "vector2", "[0.1,1]", "表示するテキスト"),
            (UnishVariableType.Vector3, "vector3", "[1,10,100]", "表示するテキスト"),
            (UnishVariableType.Color, "color", "red", "表示するテキスト"),
            (UnishVariableType.Array, "array", "( foo bar  )", "表示するテキスト"),
        };

        public override (UnishVariableType type, string name, string defVal, string info)[] Options { get; } =
        {
            (UnishVariableType.Unit, "u", null, ""),
            (UnishVariableType.String, "s", "fuga", ""),
            (UnishVariableType.Bool, "b", "true", ""),
            (UnishVariableType.Int, "i", "42", ""),
            (UnishVariableType.Float, "f", "3.14", ""),
            (UnishVariableType.Vector2, "v", "[1,2]", ""),
            (UnishVariableType.Vector3, "w", "[1.2,3.4,5.6]", ""),
            (UnishVariableType.Color, "c", "#33ccffaa", ""),
            (UnishVariableType.Array, "a", "(hoge fuga piyo nyan )", ""),
        };

        protected override async UniTask Run(Dictionary<string, UnishVariable> args,
            Dictionary<string, UnishVariable> options)
        {
            await IO.WriteLineAsync($"$# = {args["#"].S}");
            for (var i = 0; i <= args["#"].I; i++)
            {
                await IO.WriteLineAsync($"${i} = {args[$"{i}"].S}");
            }

            await IO.WriteLineAsync($"$- = {args["-"].S}");
            await IO.WriteLineAsync($"$@ = {args["@"].S}");
            await IO.WriteLineAsync($"$* = {args["*"].S}");
        }
    }
}
