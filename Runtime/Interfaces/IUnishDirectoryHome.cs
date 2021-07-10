using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishDirectoryHome : IUnishResource

    {
        /// <summary>
        ///     $"/{HomeName}" = "~"
        /// </summary>
        string HomeName { get; }

        /// <summary>
        ///     <p>Virtual home-relative path of current directory.</p>
        ///     <p>Virtual absolute path will be: $"/{HomeName}{CurrentHomeRelativePath}"</p>
        /// </summary>
        string CurrentHomeRelativePath { get; }

        bool TryFindEntry(string homeRelativePath, out bool isDirectory);

        bool TryChangeDirectory(string homeRelativePath);

        IEnumerable<(string homeRelativePath, int Depth, bool IsDirectory)> GetChilds(string homeRelativePath, int maxDepth = 0);

        void Open(string homeRelativePath);

        string Read(string homeRelativePath);

        IUniTaskAsyncEnumerable<string> ReadLines(string homeRelativePath);

        void Write(string homeRelativePath, string data);

        void Append(string homeRelativePath, string data);

        void Create(string homeRelativePath, bool isDirectory);

        void Delete(string homeRelativePath, bool isRecursive);
    }
}
