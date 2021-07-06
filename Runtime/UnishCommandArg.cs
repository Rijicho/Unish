using System;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public readonly struct UnishCommandArg
    {
        public readonly UnishCommandArgType Type;
        public readonly string s;
        public readonly bool b;
        public readonly int i;
        public readonly float f;
        public readonly Vector2 v;

        public UnishCommandArg(UnishCommandArgType type, string input) : this()
        {
            Type = type;
            s = input;
            switch (type)
            {
                case UnishCommandArgType.Bool:
                    if (!TryStrToBool(input, out b))
                        b = false;
                    break;
                case UnishCommandArgType.Int:
                    if (!int.TryParse(input, out i))
                    {
                        i = 0;
                        Type = UnishCommandArgType.Error;
                    }

                    break;
                case UnishCommandArgType.Float:
                    if (!float.TryParse(input, out f))
                    {
                        f = float.NaN;
                        Type = UnishCommandArgType.Error;
                    }

                    break;
                case UnishCommandArgType.Vector2:
                    var values = input.Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Split(',');
                    if (values.Length == 2)
                    {
                        if (float.TryParse(values[0], out var x) && float.TryParse(values[1], out var y))
                            v = new Vector2(x, y);
                        else
                            Type = UnishCommandArgType.Error;
                    }
                    else
                        Type = UnishCommandArgType.Error;

                    break;
                case UnishCommandArgType.String:
                {
                    if (string.IsNullOrEmpty(s))
                        break;
                    if (s.StartsWith("\"") && s.EndsWith("\"")) s = s.Substring(1, s.Length - 2);
                    break;
                }
            }
        }

        public static UnishCommandArg None => new UnishCommandArg(UnishCommandArgType.None, "");


        private static readonly string[] TrueStrings =
        {
            "true", "t", "y", "yes", "1", "o", "on", "enabled", "active",
        };

        private static readonly string[] FalseStrings =
        {
            "false", "f", "n", "no", "0", "x", "off", "disabled", "passive", "inactive", "",
        };

        private static bool TryStrToBool(string str, out bool b)
        {
            foreach (var ts in TrueStrings)
            {
                if (string.Equals(ts, str, StringComparison.OrdinalIgnoreCase))
                {
                    b = true;
                    return true;
                }
            }

            foreach (var fs in FalseStrings)
            {
                if (string.Equals(fs, str, StringComparison.OrdinalIgnoreCase))
                {
                    b = false;
                    return true;
                }
            }

            b = false;
            return false;
        }
    }
}