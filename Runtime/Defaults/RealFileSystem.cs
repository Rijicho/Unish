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
        public string HomeName                { get; }
        public string RealHomePath            { get; }
        public string CurrentHomeRelativePath { get; private set; }

        private IUnishEnv mEnv;
        
        public RealFileSystem(string virtualHomeName, string realHomePath)
        {
            HomeName     = virtualHomeName;
            RealHomePath = realHomePath;
        }

        public UniTask InitializeAsync(IUnishEnv env)
        {
            mEnv                    = env;
            CurrentHomeRelativePath = "";
            return default;
        }

        public UniTask FinalizeAsync(IUnishEnv env)
        {
            mEnv = null;
            return default;
        }

        public bool TryFindEntry(string homeReativePath, out bool isDirectory)
        {
            var realPath = RealHomePath + homeReativePath;
            if (Directory.Exists(realPath))
            {
                isDirectory = true;
                return true;
            }

            if (File.Exists(realPath))
            {
                isDirectory = false;
                return true;
            }

            isDirectory = false;
            return false;
        }

        public bool TryChangeDirectory(string homeRelativePath)
        {
            var realPath = RealHomePath + homeRelativePath;
            if (!Directory.Exists(realPath))
            {
                return false;
            }

            CurrentHomeRelativePath = homeRelativePath;
            mEnv.Set(BuiltInEnvKeys.WorkingDirectory, $"{PathConstants.Root}{HomeName}{CurrentHomeRelativePath}");
            return true;
        }

        public IEnumerable<(string homeRelativePath, int Depth, bool IsDirectory)> GetChilds(string homeRelativePath, int maxDepth = 0)
        {
            return GetChildsInternal(homeRelativePath, maxDepth, maxDepth);
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

        public void Delete(string homeRelativePath, bool isRecursive)
        {
            var realPath = RealHomePath + homeRelativePath;
            if (File.Exists(realPath))
            {
                File.Delete(realPath);
            }

            if (Directory.Exists(realPath))
            {
                Directory.Delete(realPath, isRecursive);
            }
        }

        private IEnumerable<(string homeRelativePath, int Depth, bool IsDirectory)> GetChildsInternal(string homeRelativePath, int maxDepth,
            int remainDepth)
        {
            var realPath = RealHomePath + homeRelativePath;
            foreach (var filePath in Directory.GetFiles(realPath))
            {
                yield return (filePath.Substring(RealHomePath.Length), maxDepth - remainDepth, false);
            }

            foreach (var dirPath in Directory.GetDirectories(realPath))
            {
                var dirPathWithoutHome = dirPath.Substring(RealHomePath.Length);
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
