using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public static class UnishExtensions
    {
        public static async UniTask<(string selected, int index, SelectionState state)> SuggestAndSelectAsync(
            this IUnishIO io,
            string searchWord,
            IEnumerable<string> candidates,
            bool enableRegex = true,
            Func<string, string> entryFormatter = default)
        {
            var list  = candidates.ToList();
            var index = list.FindIndex(x => x == searchWord);
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
                longest = Mathf.Min(longest, io.HorizontalCharCount - 10);
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

                io.WriteLineAsync("| Select index: ", "orange");

                var newInput = await io.ReadAsync();

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
