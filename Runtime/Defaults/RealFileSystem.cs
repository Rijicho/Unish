using System.Collections.Generic;
using System.IO;

namespace RUtil.Debug.Shell
{
    public class RealFileSystem : IUnishRealFileSystem
    {
        public string Home         { get; }
        public string Current      { get; private set; }
        public string RealHomePath { get; }

        public RealFileSystem(string virtualHomeName, string realHomePath)
        {
            Home         = virtualHomeName;
            RealHomePath = realHomePath;
            Current      = "";
        }

        public bool TryFindEntry(string path, out string foundPath, out bool hasChild)
        {
            var homeRelative = this.ConvertPathToHomeRelativeForm(path);
            var realPath     = RealHomePath + homeRelative;
            if (Directory.Exists(realPath))
            {
                foundPath = homeRelative;
                hasChild  = true;
                return true;
            }

            if (File.Exists(realPath))
            {
                foundPath = homeRelative;
                hasChild  = false;
                return true;
            }

            foundPath = null;
            hasChild  = false;
            return false;
        }

        public bool TryChangeDirectoryTo(string path)
        {
            var homeRelative = this.ConvertPathToHomeRelativeForm(path);
            var realPath     = RealHomePath + homeRelative;
            if (!Directory.Exists(realPath))
            {
                return false;
            }

            Current = homeRelative;
            return true;
        }


        public IEnumerable<(string path, int depth, bool hasChild)> GetChilds(string searchRoot, int depth = 0)
        {
            return GetChildsInternal(searchRoot, depth, depth);
        }

        private IEnumerable<(string path, int depth, bool hasChild)> GetChildsInternal(string searchRoot, int maxDepth,
            int remainDepth)
        {
            var realPath = RealHomePath + searchRoot;
            foreach (var filePath in Directory.GetFiles(realPath))
            {
                yield return (Path.GetFileName(this.ConvertPathToHomeRelativeForm(filePath)), maxDepth - remainDepth,
                    false);
            }

            foreach (var dirPath in Directory.GetDirectories(realPath))
            {
                var dirPathWithoutHome = Path.GetFileName(this.ConvertPathToHomeRelativeForm(dirPath));
                yield return (dirPathWithoutHome, maxDepth - remainDepth, true);
                if (remainDepth != 0)
                {
                    foreach (var elem in GetChildsInternal(dirPathWithoutHome, maxDepth, remainDepth - 1))
                    {
                        yield return elem;
                    }
                }
            }
        }
    }
}
