using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishDirectoryRoot : IUnishDirectoryRoot
    {
        public UnishDirectoryRoot(IEnumerable<IUnishDirectorySystem> directories)
        {
            mDirectories = directories.ToArray();
        }

        private readonly IUnishDirectorySystem[] mDirectories;
        public           IUnishDirectorySystem   CurrentDirectory { get; private set; }

        public UnishDirectoryEntry Current => UnishDirectoryEntry.Create(
            CurrentDirectory?.Home ?? "",
            CurrentDirectory == null ? "" : CurrentDirectory.Current,
            true);

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
                CurrentDirectory = default;
                return true;
            }

            if (!TryGetDirectorySystem(entry, out var d))
            {
                return false;
            }

            CurrentDirectory = d;
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
                    return mDirectories.Select(d => (UnishDirectoryEntry.Home(d.Home), 0));
                }

                {
                    // TryFindEntryでチェック済みなのでInvalidにはならない
                    TryGetDirectorySystem(entry, out var d);
                    return d.GetChilds(entry.HomeRelativePath)
                        .Select(x => (UnishDirectoryEntry.Create(d.Home, x.path, x.hasChild), 0));
                }
            }

            if (entry.IsRoot)
            {
                static IEnumerable<(UnishDirectoryEntry entry, int depth)> GetChildsOfHome(IUnishDirectorySystem directorySystem, int depth)
                {
                    yield return (UnishDirectoryEntry.Home(directorySystem.Home), 0);
                    if (depth > 0)
                    {
                        foreach (var entry in directorySystem.GetChilds("", depth - 1))
                        {
                            yield return (UnishDirectoryEntry.Create(directorySystem.Home, entry.path, entry.hasChild), entry.depth);
                        }
                    }
                }

                return mDirectories.SelectMany(d => GetChildsOfHome(d, depth));
            }

            {
                TryGetDirectorySystem(entry, out var d);
                return d.GetChilds(entry.HomeRelativePath, depth)
                    .Select(x => (UnishDirectoryEntry.Create(d.Home, x.path, x.hasChild), x.depth));
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
            var homeRelativePath = PathUtils.ConvertToHomeRelativePath(pathInput,
                CurrentDirectory?.Home ?? "", CurrentDirectory?.Current, CurrentDirectory?.CurrentParent(), out var home);
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

        private bool TryGetDirectorySystem(UnishDirectoryEntry entry, out IUnishDirectorySystem directory)
        {
            return TryGetDirectorySystem(entry.HomeName, out directory);
        }

        private bool TryGetDirectorySystem(string homeName, out IUnishDirectorySystem directory)
        {
            directory = mDirectories.FirstOrDefault(x => x.Home == homeName);
            return directory != default;
        }
    }
}
