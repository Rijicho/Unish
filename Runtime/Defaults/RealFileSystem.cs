using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
            var homeRelative = this.ConvertToHomeRelativePath(path);
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

        public bool TryChangeDirectory(string homeRelativePath)
        {
            var realPath = RealHomePath + homeRelativePath;
            if (!Directory.Exists(realPath))
            {
                return false;
            }

            Current = homeRelativePath;
            return true;
        }
        
        public IEnumerable<(string path, int depth, bool hasChild)> GetChilds(string searchRoot, int depth = 0)
        {
            return GetChildsInternal(searchRoot, depth, depth);
        }

        public void Open(string path)
        {
            Application.OpenURL(this.ConvertToRealPath(path));
        }

        public string Read(string path)
        {
            return File.ReadAllText(this.ConvertToRealPath(path));
        }

        public void Write(string path, string data)
        {
            File.WriteAllText(this.ConvertToRealPath(path), data);
        }

        public void Append(string path, string data)
        {
            File.AppendAllText(this.ConvertToRealPath(path), data);
        }

        public void Create(string path, bool isDirectory)
        {
            var realPath = this.ConvertToRealPath(path);
            if (isDirectory)
            {
                if (!Directory.Exists(realPath))
                {
                    Directory.CreateDirectory(realPath);
                }
            }
            else
            {
                if (!File.Exists(realPath))
                {
                    File.WriteAllBytes(this.ConvertToRealPath(realPath), Array.Empty<byte>());
                }
            }
        }

        public void Delete(string path)
        {
            var realPath = this.ConvertToRealPath(path);
            if (File.Exists(realPath))
            {
                File.Delete(realPath);
            }
        }

        private IEnumerable<(string path, int depth, bool hasChild)> GetChildsInternal(string searchRoot, int maxDepth,
            int remainDepth)
        {
            var realPath = RealHomePath + searchRoot;
            foreach (var filePath in Directory.GetFiles(realPath))
            {
                yield return (Path.GetFileName(this.ConvertToHomeRelativePath(filePath)), maxDepth - remainDepth,
                    false);
            }

            foreach (var dirPath in Directory.GetDirectories(realPath))
            {
                var dirPathWithoutHome = Path.GetFileName(this.ConvertToHomeRelativePath(dirPath));
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
