using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class DefaultUnishCommandRepository : IUnishCommandRepository
    {
        protected DefaultUnishCommandRepository()
        {
        }

        private static DefaultUnishCommandRepository mInstance;

        public static DefaultUnishCommandRepository Instance => mInstance ??= new DefaultUnishCommandRepository();

        public IReadOnlyList<UnishCommandBase> Commands => mCommands;

        private readonly List<UnishCommandBase> mCommands = new List<UnishCommandBase>();

        public IReadOnlyDictionary<string, UnishCommandBase> Map => mMap;

        private readonly Dictionary<string, UnishCommandBase> mMap = new Dictionary<string, UnishCommandBase>();

        public IDictionary<string, string> Aliases => mAliases;

        private readonly Dictionary<string, string> mAliases = new Dictionary<string, string>();


        protected virtual Assembly[] GetDomainAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private Type[] mCommandTypesCache;

        public UniTask InitializeAsync()
        {
            mMap.Clear();
            mCommands.Clear();
            mAliases.Clear();

            if (mCommandTypesCache == null)
            {
                var tCommandBase = typeof(UnishCommandBase);
                mCommandTypesCache = GetDomainAssemblies()
                    .SelectMany(asm => asm.GetTypes()
                        .Where(t => t.IsSubclassOf(tCommandBase) && !t.IsAbstract))
                    .ToArray();
            }

            foreach (var t in mCommandTypesCache)
            {
                var instance = Activator.CreateInstance(t) as UnishCommandBase;
                mCommands.Add(instance);
                foreach (var op in instance.Ops)
                {
                    mMap[op] = instance;
                }

                foreach (var alias in instance.Aliases)
                {
                    mMap["@" + alias] = instance;
                }
            }

            return default;
        }

        public UniTask FinalizeAsync()
        {
            mInstance = null;
            return default;
        }
    }
}
