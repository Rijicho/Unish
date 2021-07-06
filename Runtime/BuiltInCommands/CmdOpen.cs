using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    internal class CmdOpen : UnishCommandBase
    {
        public override string[] Ops { get; } =
        {
            "open",
        };

        public override (UnishCommandArgType type, string name, string defVal, string info)[] Params { get; } =
        {
            (UnishCommandArgType.String, "path", null, "URL or file path (CurrentDirectory: PersistentDataPath)"),
        };

        protected override UniTask Run(IUnish shell, string op, Dictionary<string, UnishCommandArg> args,
            Dictionary<string, UnishCommandArg> options)
        {
            var path = args["path"].s;
            if (Uri.TryCreate(path, UriKind.Absolute, out var result))
            {
                if (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps ||
                    result.Scheme == Uri.UriSchemeFile) Application.OpenURL(path);
            }
            else
            {
                var filePath = path.StartsWith("./")
                    ? Application.persistentDataPath + path.Substring(1)
                    : Application.persistentDataPath + "/" + path;
                if (File.Exists(filePath))
                    Application.OpenURL(filePath);
                else if (Uri.TryCreate("https://" + path, UriKind.Absolute, out _))
                    Application.OpenURL("https://" + path);
            }

            return default;
        }
    }
}