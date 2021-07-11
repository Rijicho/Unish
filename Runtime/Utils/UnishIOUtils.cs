using System.IO;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace RUtil.Debug.Shell
{
    public static class UnishIOUtils
    {
        public static UniTask WriteLineAsync(this IUnishIO io, string data, string colorCode = "white")
        {
            return io.WriteColoredAsync(data + '\n', colorCode);
        }

        public static async UniTask WriteColoredAsync(this IUnishIO io, string data, string colorCode = "white")
        {
            var lines = data.Split('\n');
            if (lines.Length == 1)
            {
                await io.WriteAsync($"<color={colorCode}>{data}</color>");
            }

            for (var i = 0; i < lines.Length; i++)
            {
                await io.WriteAsync($"<color={colorCode}>{lines[i]}</color>{(i == lines.Length - 1 ? "" : "\n")}");
            }
        }
    }
}
