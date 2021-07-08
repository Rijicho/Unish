using System.IO;
using Cysharp.Threading.Tasks;
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

        public IUniTaskAsyncEnumerable<string> ReadUnishRc()
        {
            var path = Application.persistentDataPath + "/.unishrc";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
            }

            return UnishIOUtility.ReadSourceFileLines(path);
        }

        public IUniTaskAsyncEnumerable<string> ReadUProfile()
        {
            var path = Application.persistentDataPath + "/.uprofile";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
            }

            return UnishIOUtility.ReadSourceFileLines(path);
        }
    }
}
