namespace RUtil.Debug.Shell
{
    /// <summary>
    ///     環境変数
    /// </summary>
    public class GlobalEnv : EnvBase
    {
        public override IUnishEnv Fork()
        {
            var ret = new GlobalEnv();
            foreach (var kv in this)
            {
                ret[kv.Key] = kv.Value;
            }

            return ret;
        }
    }
}
