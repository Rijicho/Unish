using System;
using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishEnv : IUnishResource, IEnumerable<KeyValuePair<string, UnishCommandArg>>
    {
        event Action<UnishCommandArg> OnSet;
        event Action<string>          OnRemoved;

        UnishCommandArg this[string key] { get; set; }

        IEnumerable<string>          Keys   { get; }
        IEnumerable<UnishCommandArg> Values { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out UnishCommandArg value);

        int Count { get; }

        void Remove(string key);
    }
}
