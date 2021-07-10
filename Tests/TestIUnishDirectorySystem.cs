using System.IO;
using NUnit.Framework;
using RUtil.Debug.Shell;
using UnityEngine;

public class TestIUnishDirectorySystem
{
    private IUnishDirectoryHome d;


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
}
