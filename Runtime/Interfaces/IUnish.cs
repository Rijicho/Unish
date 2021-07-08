using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnish
    {
        IUnishView View { get; }

        IUnishCommandRepository CommandRepository { get; }

        IColorParser ColorParser { get; }

        IUnishInputHandler InputHandler { get; }

        ITimeProvider TimeProvider { get; }

        IUnishRcRepository RcRepository { get; }
        
        IEnumerable<IUnishDirectorySystem> DirectorySystems { get; }
        
        IUnishDirectorySystem CurrentDirectorySystem { get; set; }
        
        string Prompt { get; set; }

        UniTask RunCommandAsync(string cmd);

        void WriteLine(string line);

        UniTask<string> ReadLineAsync();

        UniTask OpenAsync();

        UniTask CloseAsync();
    }
}