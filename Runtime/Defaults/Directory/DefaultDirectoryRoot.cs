using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class PathTree
    {
        public IUnishFileSystem     FileSystem { get; }
        public UnishFileSystemEntry Entry      { get; }
        public List<PathTree>       Childs     { get; }

        public PathTree(UnishFileSystemEntry entry)
        {
            Entry = entry;
            if (entry.Type == UnishFileSystemEntryType.Directory || entry.IsRoot)
            {
                Childs = new List<PathTree>();
            }
        }

        public PathTree(IUnishFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            Entry      = UnishFileSystemEntry.FileSystem(fileSystem.RootPath);
            Childs     = null;
        }

        public bool TryNext(string childName, out PathTree childTree)
        {
            if (Childs == null)
            {
                childTree = null;
                return false;
            }

            childTree = Childs.FirstOrDefault(child => child.Entry.Name == childName);
            return childTree != null;
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

            if (Childs == null)
            {
                if (Entry.IsFileSystem)
                {
                    foreach (var c in FileSystem.GetChilds("", maxDepth - depth))
                    {
                        yield return (c.Entry, c.Depth + depth);
                    }
                }

                yield break;
            }

            foreach (var child in Childs)
            {
                yield return (child.Entry, depth);
                foreach (var c in child.GetChilds(depth + 1, maxDepth))
                {
                    yield return c;
                }
            }
        }
    }

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

            mPathTree = new PathTree(UnishFileSystemEntry.Root);
            foreach (var childFileSystem in Childs)
            {
                var homePath = UnishPathUtils.SplitPath(childFileSystem.RootPath).ToArray();
                var current  = mPathTree;
                for (var i = 0; i < homePath.Length; i++)
                {
                    var entry = homePath[i];
                    var path  = current.Entry.Path + UnishPathConstants.Separator + entry;
                    var next  = current.Childs?.FirstOrDefault(x => x.Entry.Path == path);
                    if (next == default)
                    {
                        next = i == homePath.Length - 1
                            ? new PathTree(childFileSystem)
                            : new PathTree(UnishFileSystemEntry.Directory(path));
                        current.Childs.Add(next);
                    }

                    current = next;
                }
            }
        }

        public async UniTask FinalizeAsync()
        {
            foreach (var child in Childs.Reverse())
            {
                await child.FinalizeAsync();
            }

            mPathTree = null;
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
            var i              = 0;
            var consumedLength = 0;
            while (i < splitted.Length && current.TryNext(splitted[i], out var next))
            {
                current        =  next;
                consumedLength += 1 + splitted[i].Length;
                i++;
            }

            // 子ファイルシステム以外で止まっていたら
            if (current.Entry.IsRoot || !current.Entry.IsFileSystem)
            {
                foreach (var child in current.GetChilds(maxDepth))
                {
                    yield return child;
                }

                yield break;
            }

            // ファイルシステムで止まっていたら
            if (current.Entry.IsFileSystem)
            {
                foreach (var entry in current.FileSystem.GetChilds(i == splitted.Length ? "" : path.Substring(consumedLength), maxDepth))
                {
                    yield return entry;
                }
            }
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

            if (TryFindEntry(path, out var entry))
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

            if (TryFindEntry(path, out var entry))
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

            if (TryFindEntry(path, out var entry))
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

            if (TryFindEntry(path, out var entry))
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

            if (TryFindEntry(path, out var entry))
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

            if (TryFindEntry(path, out var entry))
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

            if (TryFindEntry(path, out var entry))
            {
                throw new InvalidOperationException("Virtual entries cannot be deleted.");
            }

            throw new DirectoryNotFoundException($"The entry {relativePath} does not exist.");
        }
    }
}
