using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public enum SelectionState
    {
        Succeeded,
        Canceled,
        Failed,
    }

    public static class UnishIOExtensions
    {
        public static UniTask WriteLineAsync(this UnishIOs io)
        {
            return io.WriteLineAsync("");
        }

        public static UniTask WriteLineAsync(this UnishIOs io, string data, string colorCode)
        {
            return io.WriteColoredAsync(data + '\n', colorCode);
        }

        public static UniTask WriteLineAsync(this UnishIOs io, string data)
        {
            return io.Out(data + '\n');
        }

        public static async UniTask WriteColoredAsync(this UnishIOs io, string data, string colorCode = "white")
        {
            var lines = data.Split('\n');
            if (lines.Length == 1)
            {
                await io.Out($"<color={colorCode}>{data}</color>");
            }

            var sb = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                sb.Append($"<color={colorCode}>{lines[i]}</color>{(i == lines.Length - 1 ? "" : "\n")}");
            }

            await io.Out(sb.ToString());
        }

        public static async UniTask<(string selected, int index, SelectionState state)> SuggestAndSelectAsync(
            this UnishIOs io,
            string searchWord,
            IEnumerable<string> candidates,
            bool enableRegex = true,
            Func<string, string> entryFormatter = default)
        {
            var lineWidth = io.BuiltInEnv[UnishBuiltInEnvKeys.CharCountPerLine].I;
            var list      = candidates.ToList();
            var index     = list.FindIndex(x => x == searchWord);
            if (index >= 0)
            {
                return (searchWord, index, SelectionState.Succeeded);
            }

            List<string> suggestion;
            if (searchWord == "*" || (enableRegex && searchWord == ".*"))
            {
                suggestion = list.ToList();
            }
            else
            {
                suggestion = new List<string>();
                foreach (var s in list)
                {
                    var sLower = s.ToLower();
                    if (sLower.Contains(searchWord.ToLower()))
                    {
                        suggestion.Add(s);
                    }
                    else if (enableRegex && new Regex(searchWord, RegexOptions.IgnoreCase).Match(sLower).Success)
                    {
                        suggestion.Add(s);
                    }
                    else if (searchWord.Length >= 3)
                    {
                        var dist = sLower.DamerauLevenshteinDistance(searchWord, 3);
                        if (dist >= 0)
                        {
                            suggestion.Add(s);
                        }
                    }
                }
            }

            if (suggestion.Count > 0)
            {
                var longest = suggestion.Max(x => (entryFormatter?.Invoke(x) ?? x).Length);
                longest = Mathf.Min(longest, lineWidth - 10);
                var i = 0;
                foreach (var s in suggestion)
                {
                    var iStr    = i.ToString().PadRight(suggestion.Count.ToString().Length);
                    var iColor  = i % 2 == 0 ? "#ffff55" : "#55ff55";
                    var content = (entryFormatter?.Invoke(s) ?? s).PadRight(longest);
                    var color   = i % 2 == 0 ? "#ffffaa" : "#aaffaa";
                    await io.WriteLineAsync($"| <color={iColor}>|{iStr}></color> <color={color}>{content}</color>",
                        "orange");
                    await UniTask.Yield();
                    i++;
                }

                await io.WriteLineAsync("| Select index: ", "orange");
                await io.Out("> ");

                var newInput = await io.In(false);

                if (string.IsNullOrWhiteSpace(newInput))
                {
                    return ("", -1, SelectionState.Canceled);
                }

                if (int.TryParse(newInput, out index) && 0 <= index && index < suggestion.Count)
                {
                    var key = suggestion.ElementAt(index);
                    index = list.FindIndex(x => x == key);
                    return (key, index, SelectionState.Succeeded);
                }

                if ((index = list.FindIndex(x => x == newInput)) >= 0)
                {
                    return (newInput, index, SelectionState.Succeeded);
                }

                return ("", -1, SelectionState.Failed);
            }

            return ("", -1, SelectionState.Failed);
        }
    }
}
