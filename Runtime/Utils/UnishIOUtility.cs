using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public static class UnishIOUtility
    {
        public static string ReadTextFile(string path)
        {
            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }
        
        public static async IAsyncEnumerable<string> ReadTextFileLines(string path)
        {
            if (!File.Exists(path)) yield break;

            using var reader = new StreamReader(path);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                yield return line;
            }
        }
        public static async IAsyncEnumerable<string> ReadSourceFileLines(string path)
        {
            if (!File.Exists(path)) yield break;

            using var reader = new StreamReader(path);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.StartsWith("#"))
                    continue;
                yield return line;
            }
        }

        public static bool IsValidUrlPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out var result)
                   && (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp);
        }
    }
}