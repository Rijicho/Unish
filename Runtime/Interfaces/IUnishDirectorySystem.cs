using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishDirectoryRoot
    {
        public UnishDirectoryRoot(IEnumerable<IUnishDirectorySystem> directories)
        {
            mDirectories = directories.ToArray();
        }
        private IUnishDirectorySystem[] mDirectories;
        
    }
    public interface IUnishDirectorySystem
    {
        // "/{Home}" (= "~") becomes the virtual root of the directory system
        string Home { get; }

        // virtual path of current directory
        // virtual full path will be "/{Home}{Current}" or "~{Current}"
        string Current { get; }

        bool TryFindEntry(string homeRelativePath, out bool hasChild);

        bool TryChangeDirectory(string homeRelativePath);

        IEnumerable<(string path, int depth, bool hasChild)> GetChilds(string homeRelativePath, int depth = 0);

        void Open(string homeRelativePath);

        string Read(string homeRelativePath);

        IUniTaskAsyncEnumerable<string> ReadLines(string homeRelativePath);

        void Write(string homeRelativePath, string data);

        void Append(string homeRelativePath, string data);

        void Create(string homeRelativePath, bool isDirectory);

        void Delete(string homeRelativePath);
    }
}
