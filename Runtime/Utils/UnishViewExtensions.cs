using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public static class UnishViewExtensions
    {
        public static UniTask WriteLine(this IUnishIO io, string line)
        {
            return io.WriteAsync(line + "\n");
        }
    }
}
