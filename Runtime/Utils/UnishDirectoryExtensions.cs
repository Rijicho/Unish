using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RUtil.Debug.Shell.PathConstants;

namespace RUtil.Debug.Shell
{
    public static class UnishDirectoryExtensions
    {
        public static string GetCurrentFullPath(this IUnishDirectorySystem directorySystem)
        {
            return $"{Root}{directorySystem.Home}{directorySystem.Current}";
        }

        public static IEnumerable<(string path, int depth, bool hasChild)> GetCurrentChilds(
            this IUnishDirectorySystem directorySystem, int depth)
        {
            return directorySystem.GetChilds(directorySystem.Current, depth);
        }

        public static IEnumerable<(string path, bool hasChild)> GetCurrentChilds(
            this IUnishDirectorySystem directorySystem)
        {
            return directorySystem.GetChilds(directorySystem.Current)
                .Select(x => (x.path, x.hasChild));
        }

        public static string CurrentParent(this IUnishDirectorySystem directorySystem)
        {
            return string.IsNullOrWhiteSpace(directorySystem.Current)
                ? null
                : directorySystem.Current.Substring(0, directorySystem.Current.LastIndexOf(Separator));
        }


        public static string ConvertToHomeRelativePath(this IUnishDirectorySystem directorySystem, string input)
        {
            if (input == Root)
            {
                return null;
            }

            if (input[input.Length - 1] == Separator)
            {
                input = input.Substring(0, input.Length - 1);
            }

            if (input == CurrentDir)
            {
                return directorySystem.Current;
            }

            if (input == ParentDir)
            {
                return directorySystem.CurrentParent();
            }

            if (input == Home)
            {
                return "";
            }

            //Rootから見たパスに変換
            if (input.StartsWith(Root))
            {
                input = input.Substring(Root.Length);
            }
            else if (directorySystem is IUnishRealFileSystem fileSystem && input.StartsWith(fileSystem.RealHomePath))
            {
                input = directorySystem.Home + input.Substring(fileSystem.RealHomePath.Length);
            }
            else
            {
                if (input.StartsWith(HomeRelativePrefix))
                {
                    input = directorySystem.Home + input.Substring(Home.Length);
                }
                else if (input.StartsWith(CurrentRelativePrefix))
                {
                    input = directorySystem.Home + directorySystem.Current + input.Substring(CurrentDir.Length);
                }
                else if (input.StartsWith(ParentRelativePrefix))
                {
                    input = directorySystem.Home + directorySystem.CurrentParent() + input.Substring(ParentDir.Length);
                }
                else
                {
                    input = directorySystem.Home + directorySystem.Current + Separator + input;
                }
            }

            var pathStack = new Stack<string>();

            for (int i = 0, j = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c != Separator && c != AltSeparator && i != input.Length - 1)
                {
                    continue;
                }

                var node = c == Separator || c == AltSeparator
                    ? input.Substring(j, i - j)
                    : input.Substring(j);
                switch (node)
                {
                    case CurrentDir:
                        break;
                    case ParentDir when pathStack.Count > 0:
                        pathStack.Pop();
                        break;
                    case ParentDir:
                        break;
                    case Home:
                        pathStack.Clear();
                        pathStack.Push(directorySystem.Home);
                        break;
                    default:
                        pathStack.Push(node);
                        break;
                }

                j = i + 1;
            }

            // Root
            if (pathStack.Count == 0)
            {
                return null;
            }

            // Root以外
            var sb = new StringBuilder();
            while (pathStack.Count > 1)
            {
                sb.Insert(0, pathStack.Pop());
                sb.Insert(0, Separator);
            }

            return sb.ToString();
        }

        public static string ConvertToRealPath(this IUnishRealFileSystem fileSystem, string input)
        {
            return fileSystem.RealHomePath + fileSystem.ConvertToHomeRelativePath(input);
        }

        public static bool IsRoot(this IUnishDirectorySystem directorySystem, string path)
        {
            return ConvertToHomeRelativePath(directorySystem, path) == null;
        }
    }
}
