using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishFileSystemRoot : IUnishFileSystem
    {
        IUnishFileSystem CurrentHome      { get; }
        public string    CurrentDirectory { get; }
    }
}
