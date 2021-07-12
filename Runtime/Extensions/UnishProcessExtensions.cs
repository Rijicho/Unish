using System;

namespace RUtil.Debug.Shell
{
    public static class UnishProcessExtensions
    {
        public static IUnishEnv GetGlobalEnv(this IUnishProcess process)
        {
            while (process != null && !(process is IUnishRoot))
            {
                process = process.Parent;
            }

            if (process is IUnishRoot root)
            {
                return root.GlobalEnv;
            }

            throw new Exception("This process has no root process.");
        }
    }
}
