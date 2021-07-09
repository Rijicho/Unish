using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishCommandRunner
    {
        UniTask RunCommandAsync(IUnishPresenter shell, string cmd);
        
    }
    
    public interface IUnishPresenter
    {
        IUnishView View { get; }

        IUnishCommandRepository CommandRepository { get; }
        
        IUnishCommandRunner CommandRunner { get; }

        IUnishColorParser ColorParser { get; }

        IUnishTimeProvider TimeProvider { get; }

        IUnishRcRepository RcRepository { get; }

        IEnumerable<IUnishDirectorySystem> DirectorySystems { get; }

        IUnishDirectorySystem CurrentDirectorySystem { get; set; }

        string Prompt { get; set; }


        UniTask RunAsync();
        void Halt();
    }
}
