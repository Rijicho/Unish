using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class UnishFileSystemRoot : IUnishFileSystemRoot
    {
        public IUnishEnv BuiltInEnv { private get; set; }
        public string    RootPath   => UnishPathConstants.Root;

        private PathTree mPathTree;

        protected virtual IUnishFileSystem[] Childs { get; } =
        {
            new RealFileSystem("/home/pdp", Application.persistentDataPath),
            new RealFileSystem("/home/dp", Application.dataPath),
        };

        public IUnishFileSystem CurrentHome
        {
            get
            {
                var wd = BuiltInEnv[UnishBuiltInEnvKeys.WorkingDirectory].S;
                return Childs.FirstOrDefault(child => wd.StartsWith(child.RootPath));
            }
        }

        public string CurrentDirectory => BuiltInEnv[UnishBuiltInEnvKeys.WorkingDirectory].S;


        public async UniTask InitializeAsync()
        {
            BuiltInEnv.Set(UnishBuiltInEnvKeys.HomePath, Childs[0].RootPath);
            foreach (var child in Childs)
            {
                await child.InitializeAsync();
            }

            mPathTree = new PathTree(Childs);
        }

        public async UniTask FinalizeAsync()
        {
            mPathTree = null;
            foreach (var child in Childs.Reverse())
            {
                await child.FinalizeAsync();
            }
        }

        public bool TryFindEntry(string relativePath, out UnishFileSystemEntry entry)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            if (path == UnishPathConstants.Root)
            {
                entry = UnishFileSystemEntry.Root;
                return true;
            }

            foreach (var child in Childs)
            {
                if (path == child.RootPath)
                {
                    entry = UnishFileSystemEntry.FileSystem(child.RootPath);
                    return true;
                }

                if (path.StartsWith(child.RootPath + UnishPathConstants.Separator))
                {
                    return child.TryFindEntry(path.Substring(child.RootPath.Length), out entry);
                }

                if (child.RootPath.StartsWith(path + UnishPathConstants.Separator))
                {
                    entry = UnishFileSystemEntry.Directory(path);
                    return true;
                }
            }

            entry = UnishFileSystemEntry.Invalid;
            return false;
        }

        public IEnumerable<(UnishFileSystemEntry Entry, int Depth)> GetChilds(string relativePath, int maxDepth = 0)
        {
            var path     = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            var splitted = UnishPathUtils.SplitPath(path).ToArray();

            var current = mPathTree;

            // 進めるところまで進む
            var      i              = 0;
            var      consumedLength = 0;
            PathTree next;
            while (i < splitted.Length && (next = current.Next(splitted[i])) != null)
            {
                consumedLength += 1 + splitted[i++].Length;
                current        =  next;
            }


            // 子ファイルシステムで止まっていたら
            if (current.SubFileSystem != null)
            {
                foreach (var entry in current.SubFileSystem.GetChilds(i == splitted.Length ? "" : path.Substring(consumedLength), maxDepth))
                {
                    yield return entry;
                }

                yield break;
            }

            // 子ファイルシステム以外で終端として止まっていたら
            if (i == splitted.Length)
            {
                foreach (var child in current.GetChilds(maxDepth))
                {
                    yield return child;
                }

                yield break;
            }

            throw new DirectoryNotFoundException($"The directory {path} does not exist.");
        }

        public void Open(string relativePath)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    child.Open(path.Substring(child.RootPath.Length));
                    return;
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("Virtual entries cannot be opened.");
            }

            throw new DirectoryNotFoundException($"The entry {path} does not exist.");
        }

        public string Read(string relativePath)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    return child.Read(path.Substring(child.RootPath.Length));
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("Virtual entries cannot be read.");
            }

            throw new DirectoryNotFoundException($"The entry {path} does not exist.");
        }

        public IUniTaskAsyncEnumerable<string> ReadLines(string relativePath)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    return child.ReadLines(path.Substring(child.RootPath.Length));
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("Virtual entries cannot be read.");
            }

            throw new DirectoryNotFoundException($"The entry {path} does not exist.");
        }

        public void Write(string relativePath, string data)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    child.Write(path.Substring(child.RootPath.Length), data);
                    return;
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("Virtual entries cannot be written.");
            }

            throw new DirectoryNotFoundException("Virtual files cannot be written.");
        }

        public void Append(string relativePath, string data)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    child.Append(path.Substring(child.RootPath.Length), data);
                    return;
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("Virtual entries cannot be written.");
            }

            throw new DirectoryNotFoundException($"The entry {path} does not exist.");
        }

        public void Create(string relativePath, bool isDirectory)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    child.Create(path.Substring(child.RootPath.Length), isDirectory);
                    return;
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("The directory allready exists.");
            }

            throw new DirectoryNotFoundException("Virtual files cannot be created.");
        }

        public void Delete(string relativePath, bool isRecursive)
        {
            var path = UnishPathUtils.ConvertToAbsolutePath(relativePath, CurrentDirectory, CurrentHome?.RootPath ?? RootPath);
            foreach (var child in Childs)
            {
                if (path.StartsWith(child.RootPath))
                {
                    child.Delete(path.Substring(child.RootPath.Length), isRecursive);
                    return;
                }
            }

            if (TryFindEntry(path, out _))
            {
                throw new InvalidOperationException("Virtual entries cannot be deleted.");
            }

            throw new DirectoryNotFoundException($"The entry {relativePath} does not exist.");
        }
    }
}
