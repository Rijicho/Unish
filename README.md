# Unish
A shell-like debug console on Unity runtime (for my very personal use) (WIP)

## Quick Start
1. Import [UniTask](https://github.com/Cysharp/UniTask)
2. It is recommended to import [InputSystem](https://docs.unity3d.com/ja/2019.4/Manual/com.unity.inputsystem.html) but can deal with old InputManager.
3. `git submodule add https://github.com/Rijicho/Unish.git Assets/Unish`
4. Write a script below, attach it to any GameObject and play the scene
```C#
using RUtil.Debug.Shell;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        new UnishRoot().RunAsync();
    }
}
```

## Change Font
It's highly recommended to replace the terrible default font with your beautiful font:
```C#
using RUtil.Debug.Shell;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Font font;

    private void Start()
    {
        new UnishRoot(
            new GlobalEnv(),
            new ShellEnv(),
            new DefaultIO(font),
            new DefaultInterpreter(),
            new DefaultDirectoryRoot()
        ).RunAsync();
    }
}
```
