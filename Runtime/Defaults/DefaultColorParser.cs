using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class DefaultColorParser : IUnishColorParser
    {
        private DefaultColorParser()
        {
        }

        private static DefaultColorParser mInstance;

        public static DefaultColorParser Instance => mInstance ??= new DefaultColorParser();

        public Color Parse(string str)
        {
            if (TryParse(str, out var value))
            {
                return value;
            }

            UnityEngine.Debug.LogError($"\"{str}\" cannot parse to color");
            return Color.clear;
        }

        public bool TryParse(string str, out Color value)
        {
            str = str.Replace(" ", "");
            if (ColorUtility.TryParseHtmlString(str, out var tmp))
            {
                value = tmp;
                return true;
            }

            switch (str.ToLower())
            {
                case "clear":
                case "transparent":
                    value = Color.clear;
                    return true;
            }

            try
            {
                var args = str.Split('/');
                if (args.Length == 4)
                {
                    var c = new float[4];
                    for (var i = 0; i < 4; i++)
                    {
                        c[i] = float.Parse(args[i]);
                    }

                    value = new Color(c[0], c[1], c[2], c[3]);
                    return true;
                }

                if (args.Length == 3)
                {
                    var c = new float[3];
                    for (var i = 0; i < 3; i++)
                    {
                        c[i] = float.Parse(args[i]);
                    }

                    value = new Color(c[0], c[1], c[2], 1);
                    return true;
                }

                value = Color.clear;
                return false;
            }
            catch
            {
                value = Color.clear;
                return false;
            }
        }

        public string ColorToCode(Color color)
        {
            var r = (byte)(255 * color.r);
            var g = (byte)(255 * color.g);
            var b = (byte)(255 * color.b);
            var a = (byte)(255 * color.a);
            return $"#{r:X}{g:X}{b:X}{a:X}";
        }
    }
}
