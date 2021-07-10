using System.Collections.Generic;
using System.Linq;
using static RUtil.Debug.Shell.PathConstants;

namespace RUtil.Debug.Shell
{
    public static class UnishDirectoryExtensions
    {

        public static string CurrentParent(this IUnishDirectorySystem directorySystem)
        {
            return string.IsNullOrWhiteSpace(directorySystem.Current)
                ? null
                : directorySystem.Current.Substring(0, directorySystem.Current.LastIndexOf(Separator));
        }

        public static string ConvertToHomeRelativePath(this IUnishDirectorySystem directorySystem, string input)
        {
            if (directorySystem is IUnishRealFileSystem fileSystem)
            {
                if (input.StartsWith(fileSystem.RealHomePath))
                {
                    input = fileSystem.Home + input.Substring(fileSystem.RealHomePath.Length);
                }
            }

            return PathUtils.ConvertToHomeRelativePath(input, directorySystem.Home, directorySystem.Current, directorySystem.CurrentParent(), out _);
        }


        public static IEnumerable<(UnishDirectoryEntry entry, int depth)> GetCurrentChilds(this IUnishDirectoryRoot root, int depth = 0)
        {
            return root.GetChilds(root.Current.FullPath, depth);
        }
    }
}
