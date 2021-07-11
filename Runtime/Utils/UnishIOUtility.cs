using System.IO;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace RUtil.Debug.Shell
{
    public static class UnishIOUtility
    {
        public static UniTask WriteLineAsync(this IUnishIO io, string data, string colorCode = "white")
        {
            return io.WriteColoredAsync(data + '\n');
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

        public static IUniTaskAsyncEnumerable<string> ReadSourceFileLines(string path)
        {
            return UniTaskAsyncEnumerable.Create<string>(async (writer, token) =>
            {
                if (!File.Exists(path))
                {
                    return;
                }

                using var reader = new StreamReader(path);
                string    line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    await writer.YieldAsync(line);
                }
            });
        }
    }
}
