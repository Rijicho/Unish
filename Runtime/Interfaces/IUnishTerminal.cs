using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace RUtil.Debug.Shell
{
    public interface IUnishTerminal : IUnishResourceWithEnv
    {
        IUniTaskAsyncEnumerable<string> ReadLinesAsync(bool withPrompt = false);
        UniTask WriteAsync(string text);
        UniTask WriteErrorAsync(Exception error);
        event Action OnHaltInput;
    }
}
