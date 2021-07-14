using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishFileSystem : IUnishResourceWithEnv
    {
        string RootPath { get; }

        bool TryFindEntry(string relativePath, out UnishFileSystemEntry entry);

        IEnumerable<(UnishFileSystemEntry Entry, int Depth)> GetChilds(string relativePath, int maxDepth = 0);

        void Open(string relativePath);

        string Read(string relativePath);

        IUniTaskAsyncEnumerable<string> ReadLines(string relativePath);

        void Write(string relativePath, string data);

        void Append(string relativePath, string data);

        void Create(string relativePath, bool isDirectory);

        void Delete(string relativePath, bool isRecursive);
    }
}
