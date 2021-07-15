using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RUtil.Debug.Shell;
using UnityEngine;

public class TestUnishPathUtils
{
    private static (string Input, string Want)[] tc0 =
    {
        ("/hoge/fuga/piyo", "/hoge/fuga"),
        ("/hoge/fuga", "/hoge"),
        ("/hoge", "/"),
        ("/", "/"),
    };

    [Test]
    public void _0_GetParentPath([ValueSource(nameof(tc0))] (string Input, string Want) tc)
    {
        Assert.AreEqual(tc.Want, UnishPathUtils.GetParentPath(tc.Input));
    }

    private static (string Pwd, string Home, string Input, string Want)[] tc1 =
    {
        ("/", "/", "/", "/"),
        ("/", "/", "~", "/"),
        ("/", "/", ".", "/"),
        ("/", "/", "..", "/"),
        ("/", "/", "~/", "/"),
        ("/", "/", "./", "/"),
        ("/", "/", "../", "/"),
        ("/", "/", "hoge", "/hoge"),
        ("/", "/", "hoge/fuga", "/hoge/fuga"),
        ("/", "/", "hoge/..", "/"),
        ("/", "/", "hoge/fuga/../piyo/..", "/hoge"),
        ("/", "/", "./hoge", "/hoge"),
        ("/", "/", "./hoge/fuga", "/hoge/fuga"),
        ("/", "/", "./hoge/..", "/"),
        ("/", "/", "./hoge/fuga/../piyo/..", "/hoge"),
        ("/", "/", "../hoge", "/hoge"),
        ("/", "/", "../hoge/fuga", "/hoge/fuga"),
        ("/", "/", "../hoge/..", "/"),
        ("/", "/", "../hoge/fuga/../piyo/..", "/hoge"),
        ("/", "/", "~/hoge", "/hoge"),
        ("/", "/", "~/hoge/fuga", "/hoge/fuga"),
        ("/", "/", "~/hoge/..", "/"),
        ("/", "/", "~/hoge/fuga/../piyo/..", "/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "/", "/"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "~", "/home/pdp"),
        ("/home/pdp/hoge/fuga", "/home/pdp", ".", "/home/pdp/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "..", "/home/pdp/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "~/", "/home/pdp"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "./", "/home/pdp/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "../", "/home/pdp/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "hoge", "/home/pdp/hoge/fuga/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "hoge/fuga", "/home/pdp/hoge/fuga/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "hoge/..", "/home/pdp/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "hoge/fuga/../piyo/..", "/home/pdp/hoge/fuga/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "./hoge", "/home/pdp/hoge/fuga/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "./hoge/fuga", "/home/pdp/hoge/fuga/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "./hoge/..", "/home/pdp/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "./hoge/fuga/../piyo/..", "/home/pdp/hoge/fuga/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "../hoge", "/home/pdp/hoge/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "../hoge/fuga", "/home/pdp/hoge/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "../hoge/..", "/home/pdp/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "../hoge/fuga/../piyo/..", "/home/pdp/hoge/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "~/hoge", "/home/pdp/hoge"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "~/hoge/fuga", "/home/pdp/hoge/fuga"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "~/hoge/..", "/home/pdp"),
        ("/home/pdp/hoge/fuga", "/home/pdp", "~/hoge/fuga/../piyo/..", "/home/pdp/hoge"),
    };

    [Test]
    public void _1_ConvertToAbsolutePath([ValueSource(nameof(tc1))] (string Pwd, string Home, string Input, string Want) tc)
    {
        Assert.AreEqual(tc.Want, UnishPathUtils.ConvertToAbsolutePath(tc.Input, tc.Pwd, tc.Home));
    }

    private static (string Input, string[] Want)[] tc2 =
    {
        (null, Array.Empty<string>()),
        ("/", Array.Empty<string>()),
        ("", Array.Empty<string>()),
        ("/hoge", new[]
        {
            "hoge",
        }),
        ("/hoge/", new[]
        {
            "hoge",
        }),
        ("hoge/", new[]
        {
            "hoge",
        }),
        ("hoge", new[]
        {
            "hoge",
        }),
        ("/hoge/fuga", new[]
        {
            "hoge",
            "fuga",
        }),
        ("hoge/fuga", new[]
        {
            "hoge",
            "fuga",
        }),
        ("/hoge/fuga/", new[]
        {
            "hoge",
            "fuga",
        }),
        ("hoge/fuga/", new[]
        {
            "hoge",
            "fuga",
        }),
    };

    [Test]
    public void _2_SplitPath([ValueSource(nameof(tc2))] (string Input, string[] Want) tc)
    {
        var (input, want) = tc;
        Assert.That(UnishPathUtils.SplitPath(input).ToArray(), Is.EqualTo(want));
    }
}

public class TestPathTree
{
    private PathTree mTree;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Directory.CreateDirectory(Application.persistentDataPath + "/__test/hoge/fuga/piyo/nyan/.dot/~tilde");
        Directory.CreateDirectory(Application.persistentDataPath + "/__test/hoge/fugo/piya");
        File.WriteAllText(Application.persistentDataPath + "/__test/hoge/fugo/nyo.mp4", "");
        Directory.CreateDirectory(Application.persistentDataPath + "/__test/hoga");
        File.WriteAllText(Application.persistentDataPath + "/__test/hoga/foo.txt", "");
        File.WriteAllText(Application.persistentDataPath + "/__test/bar.txt", "");
    }

    [SetUp]
    public void SetUp()
    {
        mTree = new PathTree(new[]
        {
            new RealFileSystem("home/a/s/s/e/t/s", Application.dataPath),
            new RealFileSystem("home/a/pd/ata", Application.persistentDataPath + "/__test"),
        });
    }

    [TearDown]
    public void TearDown()
    {
        mTree = null;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Directory.Delete(Application.persistentDataPath + "/__test", true);
    }


    private static (int Depth, string[] Want)[] tc0 =
    {
        (0, new[]
        {
            "home",
        }),
        (1, new[]
        {
            "home",
            "a",
        }),
        (2, new[]
        {
            "home",
            "a",
            "s",
            "pd",
        }),
        (3, new[]
        {
            "home",
            "a",
            "s",
            "s",
            "pd",
            "ata",
        }),
        (4, new[]
        {
            "home",
            "a",
            "s",
            "s",
            "e",
            "pd",
            "ata",
            "bar.txt",
            "hoga",
            "hoge",
        }),
    };

    [Test]
    public void _0_GetChilds([ValueSource(nameof(tc0))] (int Depth, string[] Want) tc)
    {
        var (depth, want) = tc;
        var result = mTree.GetChilds(depth).Select(x => x.Entry.Name).ToArray();
        Assert.That(result, Is.EqualTo(want));
    }
}
