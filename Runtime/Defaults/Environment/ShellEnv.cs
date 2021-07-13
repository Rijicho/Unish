namespace RUtil.Debug.Shell
{
    /// <summary>
    ///     シェル変数
    /// </summary>
    public class ShellEnv : EnvBase
    {
        public override IUnishEnv Fork()
        {
            return new ShellEnv();
        }
    }
}
