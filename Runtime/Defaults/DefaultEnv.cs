using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    using static UnishBuiltInEnvKeys;

    public class DefaultEnv : IUnishEnv
    {
        private Dictionary<string, string> mDictionary;
        public  int                        Count  => mDictionary.Count;
        public  IEnumerable<string>        Keys   => mDictionary.Keys;
        public  IEnumerable<string>        Values => mDictionary.Values;

        public event Action<KeyValuePair<string, string>> OnSet;
        public event Action<string>                       OnRemoved;

        public string this[string key]
        {
            get => mDictionary[key];
            set
            {
                mDictionary[key] = value;
                OnSet?.Invoke(new KeyValuePair<string, string>(key, value));
            }
        }

        private readonly (string Key, string Value)[] mInitials =
        {
            (ProfilePath, "~/.uprofile"),
            (RcPath, "~/.unishrc"),
            (Prompt, "%d $ "),
            (CharCountPerLine, "100"),
            (LineCount, "24"),
            (BgColor, "#000000cc"),
        };

        public UniTask InitializeAsync(IUnishEnv env)
        {
            mDictionary = new Dictionary<string, string>();
            foreach (var (key, value) in mInitials)
            {
                mDictionary.Add(key, value);
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

        public bool TryGetValue(string key, out string value)
        {
            return mDictionary.TryGetValue(key, out value);
        }

        public void Remove(string key)
        {
            mDictionary.Remove(key);
            OnRemoved?.Invoke(key);
        }


        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return mDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)mDictionary).GetEnumerator();
        }
    }
}
