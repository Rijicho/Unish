using System;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishIO : IUnishResource
    {
        UniTask<string> ReadAsync();
        UniTask WriteAsync(string text);
        UniTask WriteErrorAsync(Exception error);

        event Action OnHaltInput;
    }
}
