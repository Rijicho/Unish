using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishInterpreter : IUnishResource
    {
        IUnishCommandRepository Repository { get; }
        UniTask RunCommandAsync(IUnishPresenter shell, string cmd);
        IDictionary<string, string> Aliases { get; }
    }
}
