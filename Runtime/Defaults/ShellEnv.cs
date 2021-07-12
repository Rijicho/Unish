using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class ShellEnv : IUnishEnv
    {
        private readonly Dictionary<string, UnishVariable> mDictionary;
        public           int                               Count  => mDictionary.Count;
        public           IEnumerable<string>               Keys   => mDictionary.Keys;
        public           IEnumerable<UnishVariable>        Values => mDictionary.Values;

        public event Action<UnishVariable> OnSet;
        public event Action<string>        OnRemoved;


        protected virtual UnishVariable[] Initials { get; } =
        {
        };

        public UnishVariable this[string key]
        {
            get => mDictionary[key];
            set
            {
                mDictionary[key] = value;
                OnSet?.Invoke(value);
            }
        }

        public ShellEnv()
        {
            mDictionary = new Dictionary<string, UnishVariable>();
        }

        public UniTask InitializeAsync()
        {
            mDictionary.Clear();
            foreach (var arg in Initials)
            {
                mDictionary.Add(arg.Name, arg);
            }

            return default;
        }

        public UniTask FinalizeAsync()
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
