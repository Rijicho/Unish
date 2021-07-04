using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class DefaultTimeProvider : ITimeProvider
    {
        private DefaultTimeProvider()
        {
        }

        private static DefaultTimeProvider mInstance;
        public static DefaultTimeProvider Instance => mInstance ??= new DefaultTimeProvider();
        public float Now => Time.unscaledTime;
    }
}