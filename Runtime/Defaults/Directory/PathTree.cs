using System.Collections.Generic;
using System.Linq;

namespace RUtil.Debug.Shell
{
    public class PathTree
    {
        public IUnishFileSystem SubFileSystem { get; }

        private readonly UnishFileSystemEntry mEntry;
        private readonly List<PathTree>       mChilds;

        private PathTree(UnishFileSystemEntry entry)
        {
            mEntry = entry;
            if (entry.Type == UnishFileSystemEntryType.Directory || entry.IsRoot)
            {
                mChilds = new List<PathTree>();
            }
        }

        private PathTree(IUnishFileSystem subFileSystem)
        {
            SubFileSystem = subFileSystem;
            mEntry        = UnishFileSystemEntry.FileSystem(subFileSystem.RootPath);
            mChilds       = null;
        }

        public PathTree(IEnumerable<IUnishFileSystem> subFileSystems)
        {
            mEntry        = UnishFileSystemEntry.Root;
            SubFileSystem = null;
            mChilds       = new List<PathTree>();

            foreach (var childFileSystem in subFileSystems)
            {
                var homePath = UnishPathUtils.SplitPath(childFileSystem.RootPath).ToArray();
                var current  = this;
                for (var i = 0; i < homePath.Length; i++)
                {
                    var entry = homePath[i];
                    var path  = current.mEntry.Path + UnishPathConstants.Separator + entry;
                    var next  = current.mChilds?.FirstOrDefault(x => x.mEntry.Path == path);
                    if (next == default)
                    {
                        next = i == homePath.Length - 1
                            ? new PathTree(childFileSystem)
                            : new PathTree(UnishFileSystemEntry.Directory(path));
                        current.mChilds?.Add(next);
                    }

                    current = next;
                }
            }
        }

        public PathTree Next(string childName)
        {
            return mChilds?.FirstOrDefault(child => child.mEntry.Name == childName);
        }

        public IEnumerable<(UnishFileSystemEntry Entry, int depth)> GetChilds(int maxDepth)
        {
            return GetChilds(0, maxDepth);
        }

        private IEnumerable<(UnishFileSystemEntry Entry, int depth)> GetChilds(int depth, int maxDepth)
        {
            if (depth > maxDepth)
            {
                yield break;
            }

            if (mChilds == null)
            {
                if (mEntry.IsFileSystem)
                {
                    foreach (var c in SubFileSystem.GetChilds("", maxDepth - depth))
                    {
                        yield return (c.Entry, c.Depth + depth);
                    }
                }

                yield break;
            }

            foreach (var child in mChilds)
            {
                yield return (child.mEntry, depth);
                foreach (var c in child.GetChilds(depth + 1, maxDepth))
                {
                    yield return c;
                }
            }
        }
    }
}
