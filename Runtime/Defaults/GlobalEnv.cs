using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class GlobalEnv : ShellEnv
    {
        protected override UnishVariable[] Initials { get; } =
        {
            new UnishVariable(UnishBuiltInEnvKeys.ProfilePath, "~/.uprofile"),
            new UnishVariable(UnishBuiltInEnvKeys.RcPath, "~/.unishrc"),
            new UnishVariable(UnishBuiltInEnvKeys.Prompt, "%d $ "),
            new UnishVariable(UnishBuiltInEnvKeys.CharCountPerLine, 100),
            new UnishVariable(UnishBuiltInEnvKeys.LineCount, 24),
            new UnishVariable(UnishBuiltInEnvKeys.BgColor, new Color(0, 0, 0, (float)0xcc / 0xff)),
            new UnishVariable(UnishBuiltInEnvKeys.Quit, false),
        };
    }
}
