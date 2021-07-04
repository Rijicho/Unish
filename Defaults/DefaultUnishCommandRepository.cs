using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class DefaultUnishCommandRepository : IUnishCommandRepository
    {
        protected DefaultUnishCommandRepository()
        {
        }

        private static DefaultUnishCommandRepository mInstance;
        public static DefaultUnishCommandRepository Instance => mInstance ??= new DefaultUnishCommandRepository();
        public IDictionary<string, string> Aliases => mAliases;
        private Dictionary<string, string> mAliases = new Dictionary<string, string>();

        protected virtual string AliasPath =>
            $"{(Application.isEditor ? Application.dataPath : Application.persistentDataPath)}/unish_aliasdef";


        public IReadOnlyList<UnishCommandBase> Commands => mCommands;
        private readonly List<UnishCommandBase> mCommands = new List<UnishCommandBase>();

        public IReadOnlyDictionary<string, UnishCommandBase> Map => mMap;
        private readonly Dictionary<string, UnishCommandBase> mMap = new Dictionary<string, UnishCommandBase>();


        protected virtual Assembly[] GetDomainAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private Type[] mCommandTypesCache;

        public void Initialize()
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
                foreach (var op in instance.Ops) mMap[op] = instance;
                foreach (var alias in instance.Aliases) mMap["@" + alias] = instance;
            }


            LoadAlias();
        }

        public void SaveAlias()
        {
            var path = AliasPath;
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllLines(path, Aliases.Select(x => $"{x.Key} = {x.Value}"));
        }

        private void LoadAlias()
        {
            mAliases = new Dictionary<string, string>();
            if (File.Exists(AliasPath))
            {
                var lines = File.ReadAllText(AliasPath).Replace("\r", "").Split('\n');
                foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var cells = line.Split('=').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToArray();
                    if (cells.Length != 2) throw new Exception("Invalid input: " + line);
                    mAliases[cells[0]] = cells[1];
                }
            }
        }
    }
}