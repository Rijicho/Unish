using System.IO;
using NUnit.Framework;
using RUtil.Debug.Shell;
using UnityEngine;
using static RUtil.Debug.Shell.PathConstants;

public class TestIUnishDirectorySystem
{
    private IUnishDirectorySystem d;


    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Directory.CreateDirectory(Application.persistentDataPath + "/__test/hoge/fuga/piyo/nyan/.dot/~tilde");
    }

    [SetUp]
    public void SetUp()
    {
        d = new RealFileSystem("pdp", Application.persistentDataPath);
    }

    [TearDown]
    public void TearDown()
    {
        d = null;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Directory.Delete(Application.persistentDataPath + "/__test", true);
    }

    [Test] [Order(0)]
    public void _0_TestConstructor()
    {
        Assert.IsTrue(d.Current == "");
        Assert.IsTrue(d.Home == "pdp");

        Assert.AreEqual(Application.persistentDataPath, (d as IUnishRealFileSystem)?.RealHomePath);
    }


    private static (string Input, bool Success, string Current)[] tcsTryChangeDirectory =
    {
        ("", true, ""),
        ("/__test/hoge", true, "/__test/hoge"),
        ("/__test/fuga", false, ""),
        ("/__test/hoge/fuga", true, "/__test/hoge/fuga"),
        ("/__test/hoge/fuga/p", false, ""),
        ("/__test/hoge/fuga/piyo", true, "/__test/hoge/fuga/piyo"),
        ("/__test/hoge/fuga/piyo/nyan/.dot", true, "/__test/hoge/fuga/piyo/nyan/.dot"),
        ("/__test/hoge/fuga/piyo/nyan/.dot/~tilde", true, "/__test/hoge/fuga/piyo/nyan/.dot/~tilde"),
    };

    [Test] [Order(1)]
    public void _1_TryChangeDirectory([ValueSource(nameof(tcsTryChangeDirectory))]
        (string Input, bool Success, string Current) tc)
    {
        var (input, success, current) = tc;
        if (success)
        {
            Assert.IsTrue(d.TryChangeDirectory(input));
        }
        else
        {
            Assert.IsFalse(d.TryChangeDirectory(input));
        }

        Assert.AreEqual(current, d.Current);
    }

    private static (string Init, string Input, string Output)[] tcsConvertToHomeRelativePath =
    {
        ("", ParentDir, null),
        ("/__test/hoge/fuga", Root, null),
        ("/__test", $"{ParentDir}", ""),
        ("/__test", $"{ParentRelativePrefix}", ""),
        ("/__test", $"{ParentRelativePrefix}{ParentDir}", null),
        ("/__test", $"{ParentRelativePrefix}{ParentRelativePrefix}", null),
        ("", Home, ""),
        ("", CurrentDir, ""),
        ("/__test/hoge/fuga", Home, ""),
        ("/__test/hoge/fuga", CurrentDir, "/__test/hoge/fuga"),
        ("/__test/hoge/fuga", ParentDir, "/__test/hoge"),
        ("/__test/hoge/fuga", HomeRelativePrefix, ""),
        ("/__test/hoge/fuga", CurrentRelativePrefix, "/__test/hoge/fuga"),
        ("/__test/hoge/fuga", ParentRelativePrefix, "/__test/hoge"),
        ("/__test/hoge/fuga", HomeRelativePrefix + CurrentDir, ""),
        ("/__test/hoge/fuga", CurrentRelativePrefix + CurrentDir, "/__test/hoge/fuga"),
        ("/__test/hoge/fuga", ParentRelativePrefix + CurrentDir, "/__test/hoge"),
        ("/__test/hoge/fuga", HomeRelativePrefix + "foo/bar", "/foo/bar"),
        ("/__test/hoge/fuga", CurrentRelativePrefix + "foo/bar", "/__test/hoge/fuga/foo/bar"),
        ("/__test/hoge/fuga", ParentRelativePrefix + "foo/bar", "/__test/hoge/foo/bar"),
        ("/__test/hoge/fuga", $"{ParentRelativePrefix}piyo{Separator}{Home}{Separator}hoge{Separator}fuga", "/hoge/fuga"),
        ("/__test/hoge/fuga",
            $"{ParentRelativePrefix}{CurrentRelativePrefix}{CurrentRelativePrefix}{ParentRelativePrefix}hoge{Separator}{ParentRelativePrefix}fuga",
            "/__test/fuga"),
    };

    [Test] [Order(2)]
    public void _2_ConvertToHomeRelativePath([ValueSource(nameof(tcsConvertToHomeRelativePath))]
        (string Init, string Input, string Output) tc)
    {
        var (init, input, output) = tc;
        Assert.IsTrue(d.TryChangeDirectory(init));
        Assert.AreEqual(output, d.ConvertToHomeRelativePath(input));
    }
}
