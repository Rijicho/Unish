using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishInterpreter : IUnishResourceWithEnv
    {
        IUnishCommandRepository Repository { get; }
        UniTask RunCommandAsync(IUniShell shell, string cmd);
        IDictionary<string, string> Aliases { get; }
    }
}
