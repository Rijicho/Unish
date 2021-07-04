using System;
using System.Collections.Generic;
using System.Text;

namespace RUtil.Debug.Shell
{
    internal static class ListExtensions
    {
        public static string ToSingleString<T>(this IEnumerable<T> list, string separator = ", ",
            bool putSeparatorOnEnd = false, Func<T, string> toString = default)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var elem in list)
            {
                var s = elem is string tmp ? tmp : toString?.Invoke(elem) ?? elem.ToString();
                if (first)
                {
                    sb.Append(s);
                    first = false;
                    continue;
                }

                sb.Append(separator);
                sb.Append(s);
            }

            if (putSeparatorOnEnd) sb.Append(separator);
            return sb.ToString();
        }
    }
}