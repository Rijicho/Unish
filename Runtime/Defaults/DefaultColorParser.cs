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
            str = str.Replace(" ", "");
            if (ColorUtility.TryParseHtmlString(str, out var tmp))
            {
                return tmp;
            }

            switch (str.ToLower())
            {
                case "clear": return Color.clear;
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

                    return new Color(c[0], c[1], c[2], c[3]);
                }

                if (args.Length == 3)
                {
                    var c = new float[3];
                    for (var i = 0; i < 3; i++)
                    {
                        c[i] = float.Parse(args[i]);
                    }

                    return new Color(c[0], c[1], c[2], 1);
                }

                UnityEngine.Debug.LogError("Error - ColorParse() : 無効な文字列 [" + str + "]");
                return Color.clear;
            }
            catch
            {
                UnityEngine.Debug.LogError("Error - ColorParse() : 無効な文字列 [" + str + "]");
                return Color.clear;
            }
        }
    }
}
