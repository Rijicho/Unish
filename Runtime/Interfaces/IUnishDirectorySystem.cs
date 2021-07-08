using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishDirectorySystem
    {
        // "/{Home}" (= "~") becomes the virtual root of the directory system
        string Home { get; }

        // virtual path of current directory
        // virtual full path will be "/{Home}{Current}" or "~{Current}"
        string Current { get; }

        bool TryFindEntry(string path, out string fullPath, out bool hasChild);

        bool TryChangeDirectoryTo(string path);

        IEnumerable<(string path, int depth, bool hasChild)> GetChilds(string searchRoot, int depth = 0);
    }
}