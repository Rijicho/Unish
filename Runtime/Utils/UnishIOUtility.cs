using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public static class UnishIOUtility
    {
        public static async IAsyncEnumerable<string> ReadSourceFile(string path)
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

        public static string ConvertToRealAbsolutePath(string path)
        {
            return path.StartsWith("./")
                ? Application.persistentDataPath + path.Substring(1)
                : Application.persistentDataPath + "/" + path;
        }

        public static bool TryParseUnishPathToAbsoluteFilePath(string path, out string result)
        {
            if (IsValidRealAbsoluteFilePath(path))
            {
                result = path;
                return true;
            }
            var converted = ConvertToRealAbsolutePath(path);
            if (IsValidRealAbsoluteFilePath(converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        public static bool IsValidUrlPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out var result)
                   && (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp);
        }
        
        public static bool IsValidRealAbsoluteFilePath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out var result)
                   && result.Scheme == Uri.UriSchemeFile;
        }

        public static bool Exists(string unishPath)
        {
            return TryParseUnishPathToAbsoluteFilePath(unishPath, out var result) && File.Exists(result);
        }
        
        public static bool Exists(string unishPath, out string realPath)
        {
            return TryParseUnishPathToAbsoluteFilePath(unishPath, out realPath) && File.Exists(realPath);
        }
    }
}