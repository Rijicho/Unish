using System;
using System.IO;

namespace RUtil.Debug.Shell
{
    public enum UnishFileSystemEntryType
    {
        Unknown,
        File,
        Directory,
        FileSystemRoot,
    }
    public readonly struct UnishFileSystemEntry
    {
        public UnishFileSystemEntryType Type             { get; }
        public string                   Path             { get; }
        public bool                     IsFileSystem     => Type == UnishFileSystemEntryType.FileSystemRoot;
        public bool                     IsDirectory      => Type == UnishFileSystemEntryType.Directory;
        public bool                     IsRoot           => Path == UnishPathConstants.Root;
        public string                   Name             => System.IO.Path.GetFileName(Path);
        public string                   DirectoryName    => System.IO.Path.GetDirectoryName(Path);

        public static UnishFileSystemEntry Invalid => default;
        public static UnishFileSystemEntry Root    => FileSystem(UnishPathConstants.Root);

        public static UnishFileSystemEntry FileSystem(string fullPath)
        {
            return new UnishFileSystemEntry(fullPath, UnishFileSystemEntryType.FileSystemRoot);
        }

        public static UnishFileSystemEntry Directory(string fullPath)
        {
            return new UnishFileSystemEntry(fullPath, UnishFileSystemEntryType.Directory);
        }

        public static UnishFileSystemEntry File(string fullPath)
        {
            return new UnishFileSystemEntry(fullPath, UnishFileSystemEntryType.File);
        }

        public static UnishFileSystemEntry Create(string fullPath, UnishFileSystemEntryType type)
        {
            return new UnishFileSystemEntry(fullPath, type);
        }

        private UnishFileSystemEntry(string fullPath, UnishFileSystemEntryType type)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw new InvalidOperationException("Invalid path.");
            }
            Path = fullPath;
            Type = type;
        }
    }
}
