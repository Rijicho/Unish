using System.Collections.Generic;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public static class UnishEnvExtensions
    {
        public static bool TryGetValue(this IUnishEnv env, string key, out int value)
        {
            value = default;
            if (!env.TryGetValue(key, out var sValue))
            {
                return false;
            }

            return int.TryParse(sValue, out value);
        }

        public static bool TryGetValue(this IUnishEnv env, string key, out float value)
        {
            value = default;
            if (!env.TryGetValue(key, out var sValue))
            {
                return false;
            }

            return float.TryParse(sValue, out value);
        }

        public static bool TryGetValue(this IUnishEnv env, string key, out double value)
        {
            value = default;
            if (!env.TryGetValue(key, out var sValue))
            {
                return false;
            }

            return double.TryParse(sValue, out value);
        }

        public static bool TryGetValue(this IUnishEnv env, string key, out char value)
        {
            value = default;
            if (!env.TryGetValue(key, out var sValue))
            {
                return false;
            }

            return char.TryParse(sValue, out value);
        }

        public static bool TryGetValue(this IUnishEnv env, string key, out Color value, IUnishColorParser parser = default)
        {
            value = default;
            if (!env.TryGetValue(key, out var sValue))
            {
                return false;
            }

            parser ??= DefaultColorParser.Instance;
            return parser.TryParse(sValue, out value);
        }

        public static bool TryGetValue(this IUnishEnv env, string key, out List<string> values)
        {
            values = default;
            if (!env.TryGetValue(key, out var sValue))
            {
                return false;
            }

            values = UnishParseUtility.ParseArgs(sValue);
            return true;
        }
    }
}
