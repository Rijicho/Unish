using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class DefaultCommandRepository : IUnishCommandRepository
    {
        protected DefaultCommandRepository()
        {
        }

        private static DefaultCommandRepository mInstance;

        public static DefaultCommandRepository Instance => mInstance ??= new DefaultCommandRepository();

        public IReadOnlyList<UnishCommandBase> Commands => mCommands;

        private readonly List<UnishCommandBase> mCommands = new List<UnishCommandBase>();

        public IReadOnlyDictionary<string, UnishCommandBase> Map => mMap;

        private readonly Dictionary<string, UnishCommandBase> mMap = new Dictionary<string, UnishCommandBase>();


        protected virtual Assembly[] GetDomainAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private Type[] mCommandTypesCache;

        public UniTask InitializeAsync(IUnishEnv env)
        {
            mMap.Clear();
            mCommands.Clear();

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

        public UniTask FinalizeAsync(IUnishEnv env)
        {
            mInstance = null;
            return default;
        }
    }
}
