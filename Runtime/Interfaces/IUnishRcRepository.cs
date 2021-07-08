using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishRcRepository
    {
        IUniTaskAsyncEnumerable<string> ReadUnishRc();
        IUniTaskAsyncEnumerable<string> ReadUProfile();
    }
}
