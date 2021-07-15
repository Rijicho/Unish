using System.Text;
using Cysharp.Threading.Tasks;

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

            var sb = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                sb.Append($"<color={colorCode}>{lines[i]}</color>{(i == lines.Length - 1 ? "" : "\n")}");
            }

            await io.WriteAsync(sb.ToString());
        }
    }
}
