using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public readonly struct UnishVariable
    {
        public readonly UnishVariableType Type;
        public readonly string              Name;
        public readonly string              S;
        public readonly bool                B;
        public readonly int                 I;
        public readonly float               F;
        public readonly Vector2             V2;
        public readonly Vector3             V3;
        public readonly Color               C;

        public UnishVariable(string name, UnishVariableType type) : this()
        {
            Name = name;
            Type = type;
        }

        public UnishVariable(string name, string s) : this(name, UnishVariableType.String)
        {
            this.S = s;
        }

        public UnishVariable(string name, bool b) : this(name, UnishVariableType.Bool)
        {
            S      = b ? "true" : "false";
            this.B = b;
        }

        public UnishVariable(string name, int i) : this(name, UnishVariableType.Int)
        {
            S      = i.ToString();
            this.I = i;
        }


        public UnishVariable(string name, float f) : this(name, UnishVariableType.Float)
        {
            S      = f.ToString(CultureInfo.CurrentCulture);
            this.F = f;
        }


        public UnishVariable(string name, Vector2 v2) : this(name, UnishVariableType.Vector2)
        {
            S      = $"[{v2.x}, {v2.y}]";
            this.V2 = v2;
        }

        public UnishVariable(string name, Vector3 v) : this(name, UnishVariableType.Vector3)
        {
            S  = $"[{v.x}, {v.y}, {v.z}]";
            V3 = v;
        }

        public UnishVariable(string name, Color c) : this(name, UnishVariableType.Color)
        {
            S      = DefaultColorParser.Instance.ColorToCode(c);
            this.C = c;
        }

        public static UnishVariable Unit(string name)
        {
            return new UnishVariable(name, UnishVariableType.Unit, "<unit>");
        }


        public UnishVariable(string name, UnishVariableType type, string input) : this(name, type)
        {
            S = input;
            switch (type)
            {
                case UnishVariableType.Bool:
                    if (!TryStrToBool(input, out B))
                    {
                        Type = UnishVariableType.Error;
                    }

                    break;
                case UnishVariableType.Int:
                    if (!int.TryParse(input, out I))
                    {
                        Type = UnishVariableType.Error;
                    }

                    break;
                case UnishVariableType.Float:
                    if (!float.TryParse(input, out F))
                    {
                        Type = UnishVariableType.Error;
                    }

                    break;
                case UnishVariableType.String:
                    break;
                case UnishVariableType.Vector2:
                    {
                        var arr = new float[2];
                        var cnt = TryParseVector(input, arr);
                        if (cnt == 2)
                        {
                            V2 = new Vector2(arr[0], arr[1]);
                        }
                        else
                        {
                            Type = UnishVariableType.Error;
                        }
                    }
                    break;
                case UnishVariableType.Vector3:
                    {
                        var arr = new float[3];
                        var cnt = TryParseVector(input, arr);
                        if (cnt >= 2)
                        {
                            V2 = new Vector3(arr[0], arr[1], arr.Length == 3 ? arr[2] : 0);
                        }
                        else
                        {
                            Type = UnishVariableType.Error;
                        }
                    }
                    break;
                case UnishVariableType.Color:
                    if (!DefaultColorParser.Instance.TryParse(input, out C))
                    {
                        Type = UnishVariableType.Error;
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
