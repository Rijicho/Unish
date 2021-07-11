using System;
using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishEnv : IUnishResource, IEnumerable<KeyValuePair<string, UnishVariable>>
    {
        event Action<UnishVariable> OnSet;
        event Action<string>          OnRemoved;

        UnishVariable this[string key] { get; set; }

        IEnumerable<string>          Keys   { get; }
        IEnumerable<UnishVariable> Values { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out UnishVariable value);

        int Count { get; }

        void Remove(string key);
    }
}
