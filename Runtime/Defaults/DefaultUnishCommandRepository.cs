using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public void Initialize()
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
                foreach (var op in instance.Ops) mMap[op] = instance;
                foreach (var alias in instance.Aliases) mMap["@" + alias] = instance;
            }
        }
    }

    public class CharNode
    {
        public char C;
        public List<CharNode> Childs;
        public UnishCommandBase Command;

        public CharNode(char c)
        {
            C = c;
        }
    }

    public class CharTree
    {
        public CharNode Root;

        public CharTree()
        {
            Root = new CharNode(default);
        }

        public bool TryAdd(ReadOnlySpan<char> key, UnishCommandBase cmd)
        {
            var current = Root;
            foreach (var c in key)
            {
                current.Childs ??= new List<CharNode>();
                foreach (var child in current.Childs)
                {
                    if (c == child.C)
                    {
                        current = child;
                        goto FOUND;
                    }
                }

                current.Childs.Add(new CharNode(c));
                current = current.Childs[current.Childs.Count - 1];
                FOUND: ;
            }

            if (current.Command != null)
                return false;
            current.Command = cmd;
            return true;
        }

        public bool TryGet(ReadOnlySpan<char> key, out UnishCommandBase cmd)
        {
            cmd = null;
            var current = Root;
            foreach (var c in key)
            {
                if (current.Childs == null)
                    return false;
                foreach (var child in current.Childs)
                {
                    if (c == child.C)
                    {
                        current = child;
                        goto FOUND;
                    }
                }

                return false;
                FOUND: ;
            }

            cmd = current.Command;
            return cmd != null;
        }
    }
}