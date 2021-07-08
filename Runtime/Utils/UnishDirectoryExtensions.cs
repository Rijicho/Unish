using System;
using System.Collections.Generic;
using System.Linq;
using static RUtil.Debug.Shell.PathConstants;

namespace RUtil.Debug.Shell
{
    public static class UnishDirectoryExtensions
    {
        public static string GetCurrentFullPath(this IUnishDirectorySystem directorySystem)
        {
            return $"/{directorySystem.Home}{directorySystem.Current}";
        }

        public static IEnumerable<(string path, int depth, bool hasChild)> GetCurrentChilds(
            this IUnishDirectorySystem directorySystem, int depth)
        {
            return directorySystem.GetChilds(directorySystem.Current, depth);
        }
        
        public static IEnumerable<(string path, bool hasChild)> GetCurrentChilds(
            this IUnishDirectorySystem directorySystem)
        {
            return directorySystem.GetChilds(directorySystem.Current, 0)
                .Select(x => (x.path, x.hasChild));
        }

        public static string CurrentParent(this IUnishDirectorySystem directorySystem)
        {
            return string.IsNullOrWhiteSpace(directorySystem.Current)
                ? directorySystem.Current
                : directorySystem.Current.Substring(0, directorySystem.Current.LastIndexOf(Separator));
        }

        
        public static string ConvertPathToHomeRelativeForm(this IUnishDirectorySystem directorySystem, string fullPath)
        {
            if (fullPath == CurrentDir)
                return directorySystem.Current;
            if (fullPath == ParentDir)
                return directorySystem.CurrentParent();
            if (fullPath.StartsWith(CurrentRelativePrefix))
                return directorySystem.Current + fullPath.Substring(1);
            if (fullPath.StartsWith(ParentRelativePrefix))
                return directorySystem.CurrentParent() + fullPath.Substring(2);
            if (fullPath.StartsWith(directorySystem.Home))
                return fullPath.Substring(directorySystem.Home.Length);
            if (directorySystem is IUnishRealFileSystem fileSystem)
            {
                if (fullPath.StartsWith(fileSystem.RealHomePath))
                    return fullPath.Substring(fileSystem.RealHomePath.Length);
            }
            if (fullPath.StartsWith($"{Root}{directorySystem.Home}"))
                return fullPath.Substring(directorySystem.Home.Length + 2);
            return directorySystem.Current + Separator + fullPath;
        }
        
        public static bool IsRoot(this IUnishDirectorySystem directorySystem, string path)
        {
            if (path == Root)
                return true;
            if (path == CurrentDir || path == Home) return false;

            if (path == ParentDir) return string.IsNullOrEmpty(directorySystem.Current);

            var slash = Root.AsSpan();
            var tilde = Home.AsSpan();
            var dot = CurrentDir.AsSpan();
            var dots = ParentDir.AsSpan();

            var span = path.AsSpan();
            var current = directorySystem.Current.AsSpan();
            var k = 0;
            for (var i = 0; i < span.Length; i++)
            {
                var c = span[i];
                if (c == Separator || i + 1 == span.Length)
                {
                    if (i + 1 == span.Length)
                        i++;
                    //  0    k  i
                    //  aaaa/bbb/...
                    var nextDirName = span.Slice(k, i - k);

                    if (nextDirName.SequenceEqual(tilde))
                        current = ReadOnlySpan<char>.Empty;
                    else if (nextDirName.SequenceEqual(dot) || nextDirName.IsEmpty)
                    {
                        //same dir
                    }
                    else if (nextDirName.SequenceEqual(dots))
                    {
                        if (current.IsEmpty || current.SequenceEqual(slash))
                            current = slash;
                        else
                            current = current.Slice(0, current.LastIndexOf(Separator));
                    }
                    else if (current.SequenceEqual(slash))
                        current = ReadOnlySpan<char>.Empty;
                    else
                        current = string.Concat(current.ToString(), Separator.ToString(), nextDirName.ToString()).AsSpan();

                    k = i + 1;
                    i = k + 1;
                }
            }

            return current.SequenceEqual(slash);
        }
    }
}