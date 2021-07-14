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
        public string    RootPath     { get; }
        public IUnishEnv BuiltInEnv   { protected get; set; }
        public string    RealRootPath { get; }

        public RealFileSystem(string rootPath, string realRootPath)
        {
            RootPath     = rootPath;
            RealRootPath = realRootPath;
        }

        public UniTask InitializeAsync()
        {
            return default;
        }

        public UniTask FinalizeAsync()
        {
            return default;
        }


        public bool TryFindEntry(string relativePath, out UnishFileSystemEntry entry)
        {
            var realPath = RealRootPath + relativePath;
            if (Directory.Exists(realPath))
            {
                entry = UnishFileSystemEntry.Directory(RootPath + relativePath);
                return true;
            }

            if (File.Exists(realPath))
            {
                entry = UnishFileSystemEntry.File(RootPath + relativePath);
                return true;
            }

            entry = UnishFileSystemEntry.Invalid;
            return false;
        }

        public IEnumerable<(UnishFileSystemEntry Entry, int Depth)> GetChilds(string relativePath, int maxDepth = 0)
        {
            return GetChildsInternal(relativePath, maxDepth, maxDepth);
        }

        public void Open(string relativePath)
        {
            Application.OpenURL(RealRootPath + relativePath);
        }

        public string Read(string relativePath)
        {
            return File.ReadAllText(RealRootPath + relativePath);
        }

        public IUniTaskAsyncEnumerable<string> ReadLines(string relativePath)
        {
            return UniTaskAsyncEnumerable.Create<string>(async (writer, token) =>
            {
                var realPath = RealRootPath + relativePath;
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

        public void Write(string relativePath, string data)
        {
            File.WriteAllText(RealRootPath + relativePath, data);
        }

        public void Append(string relativePath, string data)
        {
            File.AppendAllText(RealRootPath + relativePath, data);
        }

        public void Create(string relativePath, bool isDirectory)
        {
            var realPath = RealRootPath + relativePath;
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

        public void Delete(string relativePath, bool isRecursive)
        {
            var realPath = RealRootPath + relativePath;
            if (File.Exists(realPath))
            {
                File.Delete(realPath);
            }

            if (Directory.Exists(realPath))
            {
                Directory.Delete(realPath, isRecursive);
            }
        }

        private IEnumerable<(UnishFileSystemEntry Entry, int Depth)> GetChildsInternal(string relativePath, int maxDepth,
            int remainDepth)
        {
            var realPath = RealRootPath + relativePath;
            foreach (var filePath in Directory.GetFiles(realPath))
            {
                var path = filePath.Substring(RealRootPath.Length);
                yield return (UnishFileSystemEntry.File(path), maxDepth - remainDepth);
            }

            foreach (var dirPath in Directory.GetDirectories(realPath))
            {
                var virtualPath = dirPath.Substring(RealRootPath.Length);
                yield return (UnishFileSystemEntry.Directory(RootPath + virtualPath), maxDepth - remainDepth);
                if (remainDepth != 0)
                {
                    foreach (var elem in GetChildsInternal(virtualPath, maxDepth, remainDepth - 1))
                    {
                        yield return elem;
                    }
                }
            }
        }
    }
}
