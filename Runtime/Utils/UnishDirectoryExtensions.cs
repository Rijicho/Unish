using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public static class UnishDirectoryExtensions
    {
        public static string GetCurrentFullPath(this IUnishDirectory directory)
        {
            return $"/{directory.Home}:{directory.Current}";
        }

        public static IEnumerable<(string path, int depth, bool hasChild)> GetCurrentChilds(
            this IUnishDirectory directory, int depth = 0)
        {
            return directory.GetChilds(directory.Current, depth);
        }
    }
}