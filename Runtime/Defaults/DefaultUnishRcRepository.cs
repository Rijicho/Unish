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

        public IAsyncEnumerable<string> ReadUnishRc()
        {
            var path = Application.persistentDataPath + "/.unishrc";
            if (!File.Exists(path)) File.WriteAllText(path, "");
            return UnishIOUtility.ReadSourceFile(path);
        }
        
        public IAsyncEnumerable<string> ReadUProfile()
        {
            var path = Application.persistentDataPath + "/.uprofile";
            if (!File.Exists(path)) File.WriteAllText(path, "");
            return UnishIOUtility.ReadSourceFile(path);
        }
    }
}