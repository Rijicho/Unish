using System;
using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishEnv : IUnishResource, IEnumerable<KeyValuePair<string, string>>
    {
        event Action<KeyValuePair<string, string>> OnSet;
        event Action<string>                       OnRemoved;

        string this[string key] { get; set; }

        IEnumerable<string> Keys   { get; }
        IEnumerable<string> Values { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out string value);

        int Count { get; }

        void Remove(string key);
    }
}
