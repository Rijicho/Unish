using System;
using System.IO;

namespace RUtil.Debug.Shell
{
    public readonly struct UnishDirectoryEntry
    {
        public string HomeName             { get; }
        public string HomeRelativePath     { get; }
        public bool   IsDirectory          { get; }
        public string FullPath             => $"{PathConstants.Root}{HomeName}{HomeRelativePath}";
        public bool   IsValid              => HomeName == null;
        public bool   IsRoot               => HomeName == "";
        public bool   IsHome               => HomeRelativePath == "";
        public string Name                 => Path.GetFileName(FullPath);
        public string Extension            => Path.GetExtension(FullPath);
        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(FullPath);

        public static UnishDirectoryEntry Invalid => default;
        public static UnishDirectoryEntry Root    => new UnishDirectoryEntry("", null, true);

        public static UnishDirectoryEntry Home(string homeName)
        {
            if (string.IsNullOrWhiteSpace(homeName))
            {
                throw new InvalidOperationException("Empty-named home cannot exist.");
            }
            return new UnishDirectoryEntry(homeName, "", true);
        }

        public static UnishDirectoryEntry Directory(string homeName, string homeRelativePath)
        {
            if (string.IsNullOrWhiteSpace(homeName))
            {
                throw new InvalidOperationException("Empty-named home cannot exist.");
            }
            return new UnishDirectoryEntry(homeName, homeRelativePath, true);
        }

        public static UnishDirectoryEntry File(string homeName, string homeRelativePath)
        {
            if (string.IsNullOrWhiteSpace(homeName))
            {
                throw new InvalidOperationException("Empty-named home cannot exist.");
            }
            if (string.IsNullOrWhiteSpace(homeRelativePath))
            {
                throw new InvalidOperationException("Home directory is not a file.");
            }
            return new UnishDirectoryEntry(homeName, homeRelativePath, false);
        }

        public static UnishDirectoryEntry Create(string homeName, string homeRelativePath, bool isDirectory)
        {
            return new UnishDirectoryEntry(homeName, homeRelativePath, isDirectory);
        }

        private UnishDirectoryEntry(string home, string homeRelativePath, bool isDirectory)
        {
            HomeName         = home;
            HomeRelativePath = homeRelativePath;
            IsDirectory      = isDirectory;
        }
    }
}
