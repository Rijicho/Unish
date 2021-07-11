using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public readonly struct UnishCommandArg
    {
        public readonly UnishCommandArgType Type;
        public readonly string              Name;
        public readonly string              s;
        public readonly bool                b;
        public readonly int                 i;
        public readonly float               f;
        public readonly Vector2             v;
        public readonly Vector3             v3;
        public readonly Color               c;

        public UnishCommandArg(string name, UnishCommandArgType type) : this()
        {
            Name = name;
            Type = type;
        }

        public UnishCommandArg(string name, string s) : this(name, UnishCommandArgType.String)
        {
            this.s = s;
        }

        public UnishCommandArg(string name, bool b) : this(name, UnishCommandArgType.Bool)
        {
            s      = b ? "true" : "false";
            this.b = b;
        }

        public UnishCommandArg(string name, int i) : this(name, UnishCommandArgType.Int)
        {
            s      = i.ToString();
            this.i = i;
        }


        public UnishCommandArg(string name, float f) : this(name, UnishCommandArgType.Float)
        {
            s      = f.ToString(CultureInfo.CurrentCulture);
            this.f = f;
        }


        public UnishCommandArg(string name, Vector2 v) : this(name, UnishCommandArgType.Vector2)
        {
            s      = $"[{v.x}, {v.y}]";
            this.v = v;
        }

        public UnishCommandArg(string name, Vector3 v) : this(name, UnishCommandArgType.Vector3)
        {
            s  = $"[{v.x}, {v.y}, {v.z}]";
            v3 = v;
        }

        public UnishCommandArg(string name, Color c) : this(name, UnishCommandArgType.Color)
        {
            s      = DefaultColorParser.Instance.ColorToCode(c);
            this.c = c;
        }

        public static UnishCommandArg Unit(string name)
        {
            return new UnishCommandArg(name, UnishCommandArgType.None, "<unit>");
        }


        public UnishCommandArg(string name, UnishCommandArgType type, string input) : this(name, type)
        {
            s = input;
            switch (type)
            {
                case UnishCommandArgType.Bool:
                    if (!TryStrToBool(input, out b))
                    {
                        Type = UnishCommandArgType.Error;
                    }

                    break;
                case UnishCommandArgType.Int:
                    if (!int.TryParse(input, out i))
                    {
                        Type = UnishCommandArgType.Error;
                    }

                    break;
                case UnishCommandArgType.Float:
                    if (!float.TryParse(input, out f))
                    {
                        Type = UnishCommandArgType.Error;
                    }

                    break;
                case UnishCommandArgType.String:
                    break;
                case UnishCommandArgType.Vector2:
                    {
                        var arr = new float[2];
                        var cnt = TryParseVector(input, arr);
                        if (cnt == 2)
                        {
                            v = new Vector2(arr[0], arr[1]);
                        }
                        else
                        {
                            Type = UnishCommandArgType.Error;
                        }
                    }
                    break;
                case UnishCommandArgType.Vector3:
                    {
                        var arr = new float[3];
                        var cnt = TryParseVector(input, arr);
                        if (cnt >= 2)
                        {
                            v = new Vector3(arr[0], arr[1], arr.Length == 3 ? arr[2] : 0);
                        }
                        else
                        {
                            Type = UnishCommandArgType.Error;
                        }
                    }
                    break;
                case UnishCommandArgType.Color:
                    if (!DefaultColorParser.Instance.TryParse(input, out c))
                    {
                        Type = UnishCommandArgType.Error;
                    }

                    break;
            }
        }

        private static readonly string[] TrueStrings =
        {
            "true",
            "t",
            "y",
            "yes",
            "1",
            "o",
            "on",
            "enabled",
            "active",
        };

        private static readonly string[] FalseStrings =
        {
            "false",
            "f",
            "n",
            "no",
            "0",
            "x",
            "off",
            "disabled",
            "passive",
            "inactive",
            "",
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

        private static int TryParseVector(string str, float[] dest)
        {
            str = str.Trim();
            if ((str[0] != '[' || str[str.Length - 1] != ']') && (str[0] != '(' || str[str.Length - 1] != ')'))
            {
                return -1;
            }

            var splited = str.Substring(1, str.Length - 2).Split(',')
                .Select(x => x.Trim())
                .ToArray();
            if (splited.Length == 0)
            {
                return -1;
            }

            var i = 0;
            for (; i < splited.Length && i < dest.Length; i++)
            {
                if (!float.TryParse(splited[i], out var f))
                {
                    return -1;
                }

                dest[i] = f;
            }

            return i;
        }
    }
}
