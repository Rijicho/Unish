using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishInterpreter : IUnishResourceWithEnv
    {
        IReadOnlyDictionary<string, UnishCommandBase> Commands { get; }
        UniTask RunCommandAsync(IUnishProcess shell, string cmd);
        IDictionary<string, string> Aliases { get; }
    }
}
