using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishRcRepository
    {
        IAsyncEnumerable<string> ReadUnishRc();
        IAsyncEnumerable<string> ReadUProfile();
    }
}