using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    using static UnishBuiltInEnvKeys;

    public class DefaultEnv : IUnishEnv
    {
        private readonly Dictionary<string, UnishVariable> mDictionary;
        public           int                               Count  => mDictionary.Count;
        public           IEnumerable<string>               Keys   => mDictionary.Keys;
        public           IEnumerable<UnishVariable>        Values => mDictionary.Values;

        public event Action<UnishVariable> OnSet;
        public event Action<string>        OnRemoved;

        public UnishVariable this[string key]
        {
            get => mDictionary[key];
            set
            {
                mDictionary[key] = value;
                OnSet?.Invoke(value);
            }
        }

        private readonly UnishVariable[] mInitials =
        {
            new UnishVariable(ProfilePath, "~/.uprofile"),
            new UnishVariable(RcPath, "~/.unishrc"),
            new UnishVariable(Prompt, "%d $ "),
            new UnishVariable(CharCountPerLine, 100),
            new UnishVariable(LineCount, 24),
            new UnishVariable(BgColor, new Color(0, 0, 0, (float)0xcc / 0xff)),
        };

        public DefaultEnv()
        {
            mDictionary = new Dictionary<string, UnishVariable>();
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

        public bool TryGetValue(string key, out UnishVariable value)
        {
            return mDictionary.TryGetValue(key, out value);
        }


        public void Remove(string key)
        {
            mDictionary.Remove(key);
            OnRemoved?.Invoke(key);
        }


        public IEnumerator<KeyValuePair<string, UnishVariable>> GetEnumerator()
        {
            return mDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)mDictionary).GetEnumerator();
        }
    }
}
