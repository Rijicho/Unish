using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishDirectoryRoot : IUnishResource
    {
        UnishDirectoryEntry Current     { get; }
        IUnishDirectoryHome CurrentHome { get; }
        bool TryFindEntry(string path, out UnishDirectoryEntry entry);
        bool TryChangeDirectory(string path);
        IEnumerable<(UnishDirectoryEntry entry, int depth)> GetChilds(string path, int depth = 0);
        void Open(string path);
        string Read(string path);
        IUniTaskAsyncEnumerable<string> ReadLines(string path);
        void Write(string path, string data);
        void Append(string path, string data);
        void Create(string path, bool isDirectory);
        void Delete(string path, bool isRecursively);
    }
}
