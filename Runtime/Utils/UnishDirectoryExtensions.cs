using System.Collections.Generic;
using System.Linq;
using static RUtil.Debug.Shell.PathConstants;

namespace RUtil.Debug.Shell
{
    public static class UnishDirectoryExtensions
    {
        public static IEnumerable<(UnishDirectoryEntry entry, int depth)> GetCurrentChilds(this IUnishDirectoryRoot root, int depth = 0)
        {
            return root.GetChilds(root.Current.FullPath, depth);
        }
    }
}
