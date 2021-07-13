using NUnit.Framework;
using RUtil.Debug.Shell;
using UnityEngine;

public class TestUnishCommandUtils
{
    private UnishEnvSet env;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
    }

    [SetUp]
    public void SetUp()
    {
        env = new UnishEnvSet(new BuiltinEnv(), new GlobalEnv(), new ShellEnv());
        env.Shell.Set("string", "hogefuga");
        env.Shell.Set("int", 10);
        env.Shell.Set("float", 3.14f);
        env.Shell.Set("bool", true);
        env.Shell.Set("boolean", false);
        env.Shell.Set("v2", new Vector2(1, 0.5f));
        env.Shell.Set("v3", new Vector3(1, 2, 3.4f));
        env.Shell.Set("color", new Color32(0xff, 0x12, 0x34, 0x56));
    }

    [TearDown]
    public void TearDown()
    {
        env = null;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    private static (string input, string want)[] tcParseVariables =
    {
        // ただの文字列
        ("hoge fuga", "hoge fuga"),
        ("$", "$"),
        ("$ ", "$ "),
        (" $", " $"),
        ("hoge$", "hoge$"),

        // 存在しない変数
        ("$hoge", ""),
        (" $hoge", " "),
        ("$hoge ", " "),
        (" $hoge ", "  "),
        ("${hoge}", ""),
        (" ${hoge}", " "),
        ("${hoge} ", " "),
        (" ${hoge} ", "  "),

        // 存在する変数 単体 型それぞれ
        ("$string", "hogefuga"),
        ("$int", "10"),
        ("$float", "3.14"),
        ("$bool", "true"),
        ("$v2", "[1,0.5]"),
        ("$v3", "[1,2,3.4]"),
        ("$color", "#FF123456"),
        ("${string}", "hogefuga"),
        ("${int}", "10"),
        ("${float}", "3.14"),
        ("${bool}", "true"),
        ("${v2}", "[1,0.5]"),
        ("${v3}", "[1,2,3.4]"),
        ("${color}", "#FF123456"),

        // 存在する変数名から始まる存在しない変数
        ("$string! hoge", " hoge"),
        ("$colorcolor a", " a"),

        // 存在する変数名から始まる存在する変数
        ("$boolean", "false"),

        // 複数変数
        ("$bool $boolean", "true false"),
        ("$bool$boolean", "truefalse"),
        ("$string piyo", "hogefuga piyo"),
        ("${string}piyo", "hogefugapiyo"),

        // クォーテーション内の変数
        ("\"$bool $boolean\"", "\"true false\""),
        ("'$bool $boolean'", "'$bool $boolean'"),
        ("'$bool\" $boolean'", "'$bool\" $boolean'"),
        ("\"'$bool $boolean'\"", "\"'true false'\""),
    };

    [Test]
    public void _0_ParseVariables([ValueSource(nameof(tcParseVariables))]
        (string input, string want) tc)
    {
        var (input, want) = tc;
        var parsed = UnishCommandUtils.ParseVariables(input, env);
        Assert.AreEqual(want, parsed);
    }

    private static (string input, (string, bool)[] want)[] tcSplitCommand =
    {
        ("hoge", new[]
        {
            ("hoge", false),
        }),
        ("hoge fuga", new[]
        {
            ("hoge", false),
            ("fuga", false),
        }),
        ("hoge   fuga", new[]
        {
            ("hoge", false),
            ("fuga", false),
        }),
        ("hoge   fuga piyo", new[]
        {
            ("hoge", false),
            ("fuga", false),
            ("piyo", false),
        }),
        ("hoge   'fuga piyo'", new[]
        {
            ("hoge", false),
            ("fuga piyo", false),
        }),
        ("hoge   \"fuga piyo\"", new[]
        {
            ("hoge", false),
            ("fuga piyo", false),
        }),
        ("hoge   \"fuga piyo'", new[]
        {
            ("hoge", false),
            ("\"fuga", false),
            ("piyo'", false),
        }),
        ("hoge   'fuga piyo\"", new[]
        {
            ("hoge", false),
            ("'fuga", false),
            ("piyo\"", false),
        }),
        ("\"hoge   'fuga piyo\"", new[]
        {
            ("hoge   'fuga piyo", false),
        }),
        ("alias 'hoge=\"fuga piyo\"'", new[]
        {
            ("alias", false),
            ("hoge=\"fuga piyo\"", false),
        }),
        ("ls -l 'hoge fuga' -a", new[]
        {
            ("ls", false),
            ("-l", true),
            ("hoge fuga", false),
            ("-a", true),
        }),
    };

    [Test]
    public void _1_SplitCommand([ValueSource(nameof(tcSplitCommand))] (string input, (string, bool)[] want) tc)
    {
        var (input, want) = tc;
        var splited = UnishCommandUtils.SplitCommand(input);
        Assert.AreEqual(want, splited.ToArray());
    }
}
