using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
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
}