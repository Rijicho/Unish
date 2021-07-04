using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishRcRepository
    {
        IAsyncEnumerable<string> LoadUnishRc();
        IAsyncEnumerable<string> LoadUProfile();
    }
}