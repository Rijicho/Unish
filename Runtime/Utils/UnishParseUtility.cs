using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public static class UnishParseUtility
    {
        public static List<string> ParseArgs(string input)
        {
            var ret = new List<string>();
            ParseArgs(input, ret);
            return ret;
        }

        private static void ParseArgs(string input, List<string> leading)
        {
            var tokenStartedIndex = -1;
            var isInStringSingle  = false;
            var isInStringDouble  = false;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                switch (c)
                {
                    case '"' when !isInStringSingle:
                        isInStringDouble = !isInStringDouble;
                        break;
                    case '\'' when !isInStringDouble:
                        isInStringSingle = !isInStringSingle;
                        break;
                    case var space when char.IsWhiteSpace(space) && !isInStringSingle && !isInStringDouble:
                        if (tokenStartedIndex >= 0)
                        {
                            leading.Add(input.Substring(tokenStartedIndex, i - tokenStartedIndex));
                            ParseArgs(input.Substring(i + 1), leading);
                            return;
                        }

                        break;
                    default:
                        if (tokenStartedIndex < 0)
                        {
                            tokenStartedIndex = i;
                        }

                        break;
                }
            }
        }
    }
}
