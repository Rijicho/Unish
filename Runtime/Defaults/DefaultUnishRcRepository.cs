using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class DefaultUnishRcRepository : IUnishRcRepository
    {
        private DefaultUnishRcRepository()
        {
        }

        private static DefaultUnishRcRepository mInstance;
        public static DefaultUnishRcRepository Instance => mInstance ??= new DefaultUnishRcRepository();

        public IAsyncEnumerable<string> LoadUnishRc()
        {
            var path = Application.persistentDataPath + "/.unishrc";
            return ReadFile(path);
        }
        
        public IAsyncEnumerable<string> LoadUProfile()
        {
            var path = Application.persistentDataPath + "/.uprofile";
            return ReadFile(path);
        }

        private async IAsyncEnumerable<string> ReadFile(string path)
        {
            if (!File.Exists(path)) File.WriteAllText(path, "");

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
    }
}