using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class DefaultDirectoryRoot : IUnishDirectoryRoot
    {
        private IUnishDirectoryHome[] mDirectories;
        public           IUnishDirectoryHome   CurrentHome { get; private set; }

        public UnishDirectoryEntry Current => UnishDirectoryEntry.Create(
            CurrentHome?.HomeName ?? "",
            CurrentHome == null ? "" : CurrentHome.CurrentHomeRelativePath,
            true);

        public UniTask InitializeAsync()
        {
            mDirectories = new[]
            {
                new RealFileSystem("PersistentData", Application.persistentDataPath),
            };
            CurrentHome = mDirectories[0];

            return UniTask.WhenAll(mDirectories.Select(d => d.InitializeAsync()));
        }

        public UniTask FinalizeAsync()
        {
            CurrentHome  = null;
            mDirectories = null;
            return UniTask.WhenAll(mDirectories.Select(d => d.FinalizeAsync()));;
        }

        public bool TryFindEntry(string path, out UnishDirectoryEntry entry)
        {
            var tmpEntry = ParsePathInput(path, null);
            if (tmpEntry.IsRoot)
            {
                entry = tmpEntry;
                return true;
            }


            var homeName         = tmpEntry.HomeName;
            var homeRelativePath = tmpEntry.HomeRelativePath;

            if (!TryGetDirectorySystem(homeName, out var d))
            {
                entry = UnishDirectoryEntry.Invalid;
                return false;
            }

            if (d.TryFindEntry(homeRelativePath, out var isDirectory))
            {
                entry = UnishDirectoryEntry.Create(homeName, homeRelativePath, isDirectory);
                return true;
            }

            entry = UnishDirectoryEntry.Invalid;
            return false;
        }

        public bool TryChangeDirectory(string path)
        {
            if (!TryFindEntry(path, out var entry))
            {
                return false;
            }

            if (entry.IsRoot)
            {
                CurrentHome = default;
                return true;
            }

            if (!TryGetDirectorySystem(entry, out var d))
            {
                return false;
            }

            CurrentHome = d;
            return d.TryChangeDirectory(entry.HomeRelativePath);
        }

        public IEnumerable<(UnishDirectoryEntry entry, int depth)> GetChilds(string path, int depth = 0)
        {
            if (!TryFindEntry(path, out var entry))
            {
                throw new DirectoryNotFoundException($"The directory \"{entry.FullPath}\" is not found.");
            }


            if (depth == 0)
            {
                if (entry.IsRoot)
                {
                    return mDirectories.Select(d => (UnishDirectoryEntry.Home(d.HomeName), 0));
                }

                {
                    // TryFindEntryでチェック済みなのでInvalidにはならない
                    TryGetDirectorySystem(entry, out var d);
                    return d.GetChilds(entry.HomeRelativePath)
                        .Select(x => (UnishDirectoryEntry.Create(d.HomeName, x.homeRelativePath, x.IsDirectory), 0));
                }
            }

            if (entry.IsRoot)
            {
                static IEnumerable<(UnishDirectoryEntry entry, int depth)> GetChildsOfHome(IUnishDirectoryHome directorySystem, int depth)
                {
                    yield return (UnishDirectoryEntry.Home(directorySystem.HomeName), 0);
                    if (depth > 0)
                    {
                        foreach (var entry in directorySystem.GetChilds("", depth - 1))
                        {
                            yield return (UnishDirectoryEntry.Create(directorySystem.HomeName, entry.homeRelativePath, entry.IsDirectory), depth: entry.Depth);
                        }
                    }
                }

                return mDirectories.SelectMany(d => GetChildsOfHome(d, depth));
            }

            {
                TryGetDirectorySystem(entry, out var d);
                return d.GetChilds(entry.HomeRelativePath, depth)
                    .Select(x => (UnishDirectoryEntry.Create(d.HomeName, x.homeRelativePath, x.IsDirectory), depth: x.Depth));
            }
        }

        public void Open(string path)
        {
            if (!TryFindEntry(path, out var entry))
            {
                throw new FileNotFoundException($"The entry {entry.FullPath} does not exist.");
            }

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be opened.");
            }

            TryGetDirectorySystem(entry, out var d);
            d.Open(entry.HomeRelativePath);
        }

        public string Read(string path)
        {
            if (!TryFindEntry(path, out var entry))
            {
                throw new FileNotFoundException($"The entry {entry.FullPath} does not exist.");
            }

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be read.");
            }

            TryGetDirectorySystem(entry, out var d);
            return d.Read(entry.HomeRelativePath);
        }

        public IUniTaskAsyncEnumerable<string> ReadLines(string path)
        {
            if (!TryFindEntry(path, out var entry))
            {
                throw new FileNotFoundException($"The entry {entry.FullPath} does not exist.");
            }

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be read.");
            }

            TryGetDirectorySystem(entry, out var d);
            return d.ReadLines(entry.HomeRelativePath);
        }

        public void Write(string path, string data)
        {
            var entry = ParsePathInput(path, false);

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be written.");
            }

            if (!TryGetDirectorySystem(entry.HomeName, out var d))
            {
                throw new DirectoryNotFoundException($"The directory {entry.HomeName} does not exist.");
            }

            d.Write(entry.HomeRelativePath, data);
        }

        public void Append(string path, string data)
        {
            var entry = ParsePathInput(path, false);

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be written.");
            }

            if (!TryGetDirectorySystem(entry.HomeName, out var d))
            {
                throw new DirectoryNotFoundException($"The directory {entry.HomeName} does not exist.");
            }

            d.Append(entry.HomeRelativePath, data);
        }

        public void Create(string path, bool isDirectory)
        {
            var entry = ParsePathInput(path, isDirectory);

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be written.");
            }

            if (!TryGetDirectorySystem(entry.HomeName, out var d))
            {
                throw new DirectoryNotFoundException($"The directory {entry.HomeName} does not exist.");
            }

            d.Create(entry.HomeRelativePath, isDirectory);
        }

        public void Delete(string path, bool isRecursive)
        {
            if (!TryFindEntry(path, out var entry))
            {
                throw new FileNotFoundException($"The entry {entry.FullPath} does not exist.");
            }

            if (entry.IsRoot)
            {
                throw new InvalidOperationException("The virtual root cannot be deleted.");
            }

            if (entry.IsHome)
            {
                throw new InvalidOperationException("The virtual home cannot be deleted.");
            }

            TryGetDirectorySystem(entry, out var d);
            d.Delete(entry.HomeRelativePath, isRecursive);
        }

        private UnishDirectoryEntry ParsePathInput(string pathInput, bool? isDirectoryExpected)
        {
            var currentParent = string.IsNullOrWhiteSpace(CurrentHome?.CurrentHomeRelativePath)
                ? null
                : CurrentHome.CurrentHomeRelativePath.Substring(0, CurrentHome.CurrentHomeRelativePath.LastIndexOf(PathConstants.Separator));
            var homeRelativePath = PathUtils.ConvertToHomeRelativePath(pathInput,
                CurrentHome?.HomeName ?? "", CurrentHome?.CurrentHomeRelativePath, currentParent, out var home);
            if (home == "")
            {
                return UnishDirectoryEntry.Root;
            }

            if (homeRelativePath == "")
            {
                return UnishDirectoryEntry.Home(home);
            }

            return UnishDirectoryEntry.Create(home, homeRelativePath, isDirectoryExpected ?? !Path.HasExtension(homeRelativePath));
        }

        private bool TryGetDirectorySystem(UnishDirectoryEntry entry, out IUnishDirectoryHome directory)
        {
            return TryGetDirectorySystem(entry.HomeName, out directory);
        }

        private bool TryGetDirectorySystem(string homeName, out IUnishDirectoryHome directory)
        {
            directory = mDirectories.FirstOrDefault(x => x.HomeName == homeName);
            return directory != default;
        }
    }
}
