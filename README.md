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
        new Unish().Run();
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
        new Unish(terminal: new DefaultTerminal(font)).Run();
    }
}
```

## Usage
- `h` shows the brief help.
- Input `lc -d` to show all commands in detail.
- `q` closes the window.
- Ctrl(Command)+Arrows scroll the window. 
- Window size and font size can be changed by environment variables:
  - UNISH_CHARCNT_PER_LINE
  - UNISH_LINECNT
  - UNISH_FONTSIZE
- If you want to save the settings, please put .unishrc and .uprofile (like zsh's .zshrc and .zprofile) at `Application.PersistentDataPath`.
  - The default home path can be changed by overriding Unish's directory system by script. 
