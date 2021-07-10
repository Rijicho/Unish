using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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

        public bool TryFindEntry(string homeReativePath, out bool hasChild)
        {
            var realPath = RealHomePath + homeReativePath;
            if (Directory.Exists(realPath))
            {
                hasChild = true;
                return true;
            }

            if (File.Exists(realPath))
            {
                hasChild = false;
                return true;
            }

            hasChild = false;
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

        public IEnumerable<(string path, int depth, bool hasChild)> GetChilds(string homeRelativePath, int depth = 0)
        {
            return GetChildsInternal(homeRelativePath, depth, depth);
        }

        public void Open(string homeRelativePath)
        {
            Application.OpenURL(RealHomePath + homeRelativePath);
        }

        public string Read(string homeRelativePath)
        {
            return File.ReadAllText(RealHomePath + homeRelativePath);
        }

        public IUniTaskAsyncEnumerable<string> ReadLines(string homeRelativePath)
        {
            return UniTaskAsyncEnumerable.Create<string>(async (writer, token) =>
            {
                var realPath = RealHomePath + homeRelativePath;
                if (!File.Exists(realPath))
                {
                    return;
                }

                using var reader = new StreamReader(realPath);
                string    line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    await writer.YieldAsync(line);
                }
            });
        }

        public void Write(string homeRelativePath, string data)
        {
            File.WriteAllText(RealHomePath + homeRelativePath, data);
        }

        public void Append(string homeRelativePath, string data)
        {
            File.AppendAllText(RealHomePath + homeRelativePath, data);
        }

        public void Create(string homeRelativePath, bool isDirectory)
        {
            var realPath = RealHomePath + homeRelativePath;
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
                    File.WriteAllBytes(realPath, Array.Empty<byte>());
                }
            }
        }

        public void Delete(string homeRelativePath)
        {
            var realPath = RealHomePath + homeRelativePath;
            if (File.Exists(realPath))
            {
                File.Delete(realPath);
            }
        }

        private IEnumerable<(string path, int depth, bool hasChild)> GetChildsInternal(string homeRelativePath, int maxDepth,
            int remainDepth)
        {
            var realPath = RealHomePath + homeRelativePath;
            foreach (var filePath in Directory.GetFiles(realPath))
            {
                yield return (this.ConvertToHomeRelativePath(filePath), maxDepth - remainDepth, false);
            }

            foreach (var dirPath in Directory.GetDirectories(realPath))
            {
                var dirPathWithoutHome = this.ConvertToHomeRelativePath(dirPath);
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
