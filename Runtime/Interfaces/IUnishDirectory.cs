﻿using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class PersistentDataDirectory : IUnishDirectory, IUnishFileSystem
    {
        private static PersistentDataDirectory mInstance;
        public static PersistentDataDirectory Instance => mInstance ??= new PersistentDataDirectory();
        public string Home => "PersistentDatapath";
        public string Current { get; private set; }

        public string CurrentParent => Current.Substring(0, Current.LastIndexOf('/'));

        public string RealHomePath { get; }

        private PersistentDataDirectory()
        {
            RealHomePath = Application.persistentDataPath;
        }

        public bool TryFindEntry(string path, out string foundPath, out bool hasChild)
        {
            var realPath = RealHomePath + WithoutHome(path);
            if (Directory.Exists(realPath))
            {
                foundPath = WithoutHome(realPath);
                hasChild = true;
                return true;
            }

            if (File.Exists(realPath))
            {
                foundPath = WithoutHome(realPath);
                hasChild = false;
                return true;
            }

            foundPath = null;
            hasChild = false;
            return false;
        }

        public bool TryChangeCurrentDirectoryTo(string path)
        {
            var realPath = RealHomePath + WithoutHome(path);
            if (!Directory.Exists(realPath)) return false;
            Current = WithoutHome(realPath);
            return true;
        }

        private string WithoutHome(string fullPath)
        {
            if (fullPath == ".")
                return Current;
            if (fullPath == "..")
                return CurrentParent;
            if (fullPath.StartsWith("./"))
                return Current + fullPath.Substring(1);
            if (fullPath.StartsWith("../"))
                return CurrentParent + fullPath.Substring(2);
            if (fullPath.StartsWith(UnishDirectoryUtility.HomeAlias))
                return fullPath.Substring(UnishDirectoryUtility.HomeAlias.Length);
            if (fullPath.StartsWith(RealHomePath))
                return fullPath.Substring(RealHomePath.Length);
            if (fullPath.StartsWith($"/{Home}:"))
                return fullPath.Substring(Home.Length + 2);
            return Current + "/" + fullPath;
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
                yield return (WithoutHome(filePath), maxDepth - remainDepth, false);

            foreach (var dirPath in Directory.GetDirectories(realPath))
            {
                var dirPathWithoutHome = WithoutHome(dirPath);
                yield return (dirPathWithoutHome, maxDepth - remainDepth, true);
                if (remainDepth != 0)
                {
                    foreach (var elem in GetChildsInternal(dirPathWithoutHome, maxDepth, remainDepth - 1))
                        yield return elem;
                }
            }
        }
    }

    public delegate UniTask FileSystemEntryTask(string path, bool hasChild);

    public interface IUnishDirectory
    {
        // "/{Home}:" becomes the virtual path of the directory
        string Home { get; }

        // virtual path of current directory
        // full path will be "/{Home}:{Current}" or "~{Current}"
        string Current { get; }

        bool TryFindEntry(string path, out string fullPath, out bool hasChild);

        bool TryChangeCurrentDirectoryTo(string path);

        IEnumerable<(string path, int depth, bool hasChild)> GetChilds(string searchRoot, int depth = 0);
    }

    public interface IUnishFileSystem
    {
        string RealHomePath { get; }
    }

    public static class UnishDirectoryExtensions
    {
        public static string GetCurrentFullPath(this IUnishDirectory directory)
        {
            return $"/{directory.Home}:{directory.Current}";
        }

        public static IEnumerable<(string path, int depth, bool hasChild)> GetCurrentChilds(
            this IUnishDirectory directory, int depth = 0)
        {
            return directory.GetChilds(directory.Current, depth);
        }
    }

    public static class UnishDirectoryUtility
    {
        public static string HomeAlias { get; set; } = "~";
    }
}