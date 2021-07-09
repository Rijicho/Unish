using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishPresenter
    {
        IUnishView View { get; }

        IUnishCommandRepository CommandRepository { get; }

        IUnishColorParser ColorParser { get; }

        IUnishTimeProvider TimeProvider { get; }

        IUnishRcRepository RcRepository { get; }

        IEnumerable<IUnishDirectorySystem> DirectorySystems { get; }

        IUnishDirectorySystem CurrentDirectorySystem { get; set; }

        string Prompt { get; set; }

        UniTask RunCommandAsync(string cmd);

        UniTask RunAsync();
        void Halt();
    }
}
