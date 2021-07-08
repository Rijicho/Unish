using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace RUtil.Debug.Shell
{
    public static class UnishIOUtility
    {
        public static string ReadTextFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            return File.ReadAllText(path);
        }

        public static IUniTaskAsyncEnumerable<string> ReadTextFileLines(string path)
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
                    await writer.YieldAsync(line);
                }
            });
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

        public static bool IsValidUrlPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out var result)
                   && (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp);
        }
    }
}
