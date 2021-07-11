using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    using static BuiltInEnvKeys;

    public class DefaultEnv : IUnishEnv
    {
        private readonly Dictionary<string, UnishCommandArg> mDictionary;
        public           int                                 Count  => mDictionary.Count;
        public           IEnumerable<string>                 Keys   => mDictionary.Keys;
        public           IEnumerable<UnishCommandArg>        Values => mDictionary.Values;

        public event Action<UnishCommandArg> OnSet;
        public event Action<string>          OnRemoved;

        public UnishCommandArg this[string key]
        {
            get => mDictionary[key];
            set
            {
                mDictionary[key] = value;
                OnSet?.Invoke(value);
            }
        }

        private readonly UnishCommandArg[] mInitials =
        {
            new UnishCommandArg(ProfilePath, "~/.uprofile"),
            new UnishCommandArg(RcPath, "~/.unishrc"),
            new UnishCommandArg(Prompt, "%d $ "),
            new UnishCommandArg(CharCountPerLine, 100),
            new UnishCommandArg(LineCount, 24),
            new UnishCommandArg(BgColor, new Color(0, 0, 0, (float)0xcc / 0xff)),
        };

        public DefaultEnv()
        {
            mDictionary = new Dictionary<string, UnishCommandArg>();
        }

        public UniTask InitializeAsync(IUnishEnv env)
        {
            foreach (var arg in mInitials)
            {
                mDictionary.Add(arg.Name, arg);
            }

            return default;
        }

        public UniTask FinalizeAsync(IUnishEnv env)
        {
            mDictionary.Clear();
            return default;
        }

        public bool ContainsKey(string key)
        {
            return mDictionary.ContainsKey(key);
        }

        public bool TryGetValue(string key, out UnishCommandArg value)
        {
            return mDictionary.TryGetValue(key, out value);
        }


        public void Remove(string key)
        {
            mDictionary.Remove(key);
            OnRemoved?.Invoke(key);
        }


        public IEnumerator<KeyValuePair<string, UnishCommandArg>> GetEnumerator()
        {
            return mDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)mDictionary).GetEnumerator();
        }
    }
}
