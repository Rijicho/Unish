using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RUtil.Debug.Shell
{
    public enum UnishCommandTokenType
    {
        Invalid,
        Param,
        Option,
        RedirectIn,
        RedirectOut,
        RedirectOutAppend,
        RedirectErr,
        RedirectErrAppend,
    }

    public static class UnishCommandUtils
    {
        public static string RemoveQuotesIfExist(string token)
        {
            if ((token[0] == '"' && token[token.Length - 1] == '"')
                || (token[0] == '\'' && token[token.Length - 1] == '\''))
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }

        private static string ParseVariables(string input, UnishEnvSet env, bool isInsideDoubleQuote)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.Contains('$'))
            {
                return input;
            }

            var sb          = new StringBuilder();
            var appendBegin = 0;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                // シングルクォート内部
                if (c == '\'')
                {
                    var endQuote = input.IndexOf('\'', i + 1);
                    if (endQuote >= 0)
                    {
                        // -----       
                        //      a    i     e
                        // $fuga hoge'$fuga'

                        var insideQuote = input.Substring(i + 1, endQuote - i - 1);
                        // 外側のダブルクォート内部ならパース
                        if (isInsideDoubleQuote)
                        {
                            insideQuote = ParseVariables(insideQuote, env, true);
                        }

                        // -----======
                        //      a    i     e
                        // $fuga hoge'$fuga'
                        sb.Append(input.Substring(appendBegin, i - appendBegin + 1));
                        // -----------=====
                        //      a    i     e
                        // $fuga hoge'$fuga'
                        sb.Append(insideQuote);
                        // ----------------
                        //                 i=a
                        // $fuga hoge'$fuga'
                        i = appendBegin = endQuote;
                    }

                    continue;
                }

                // ダブルクォート内部は再帰呼び出しでパース（ダブルクォート内部にダブルクォートはない）
                if (!isInsideDoubleQuote && c == '"')
                {
                    var endQuote = input.IndexOf('"', i + 1);
                    if (endQuote >= 0)
                    {
                        // -----       
                        //      a    i     e
                        // $fuga hoge"$fuga"

                        var insideQuote       = input.Substring(i + 1, endQuote - i - 1);
                        var parsedInsideQuote = ParseVariables(insideQuote, env, true);

                        // -----======
                        //      a    i     e
                        // $fuga hoge"$fuga"
                        sb.Append(input.Substring(appendBegin, i - appendBegin + 1));
                        // -----------=====
                        //      a    i     e
                        // $fuga hoge"$fuga"
                        sb.Append(parsedInsideQuote);
                        // ----------------
                        //                 i=a
                        // $fuga hoge"$fuga"
                        i = appendBegin = endQuote;
                    }

                    continue;
                }

                // $までスキップ
                if (c != '$')
                {
                    continue;
                }

                // 終端の$は無視
                if (i == input.Length - 1)
                {
                    break;
                }

                // $の次に空白がある場合は無視
                if (char.IsWhiteSpace(input[i + 1]))
                {
                    continue;
                }

                // ${x} 型
                if (input[i + 1] == '{' && i + 3 < input.Length)
                {
                    sb.Append(input.Substring(appendBegin, i - appendBegin));
                    var beginIdx = i + 2;
                    var endIdx   = input.IndexOf('}', i + 3);
                    if (endIdx < 0)
                    {
                        continue;
                    }

                    var varname = input.Substring(beginIdx, endIdx - beginIdx);
                    if (env.BuiltIn.TryGetValue(varname, out var value)
                        || env.Environment.TryGetValue(varname, out value)
                        || env.Shell.TryGetValue(varname, out value))
                    {
                        sb.Append(value.S);
                    }

                    appendBegin = endIdx + 1;
                    i           = appendBegin;
                }
                // $hoge 型
                else if (input[i + 1] != '{')
                {
                    sb.Append(input.Substring(appendBegin, i - appendBegin));
                    var beginIdx = ++i;
                    c = input[i];
                    while (!char.IsWhiteSpace(c) && c != '$' && i < input.Length)
                    {
                        if (++i < input.Length)
                        {
                            c = input[i];
                        }
                    }

                    var varname = beginIdx == i ? ""
                        : i == input.Length ? input.Substring(beginIdx)
                        : input.Substring(beginIdx, i - beginIdx);

                    if (env.BuiltIn.TryGetValue(varname, out var value)
                        || env.Environment.TryGetValue(varname, out value)
                        || env.Shell.TryGetValue(varname, out value))
                    {
                        sb.Append(value.S);
                    }

                    appendBegin = i;
                    if (c == '$')
                    {
                        i--;
                    }
                }
            }

            if (appendBegin < input.Length)
            {
                sb.Append(input.Substring(appendBegin));
            }

            return sb.ToString();
        }

        public static string ParseVariables(string input, UnishEnvSet env)
        {
            return ParseVariables(input, env, false);
        }

        public static List<(string Token, UnishCommandTokenType Type)> CommandToTokens(string input)
        {
            var ret = new List<string>();
            SplitCommandInternal(input, ret);


            var tokens = ret.Select(t =>
            {
                var type  = UnishCommandTokenType.Invalid;
                var token = t;
                if (t[0] == '-')
                {
                    type  = UnishCommandTokenType.Option;
                    token = token.TrimStart('-');
                }
                else if (t.StartsWith("<"))
                {
                    type  = UnishCommandTokenType.RedirectIn;
                    token = t.Length == 1 ? "" : t.Substring(1);
                }
                else if (t.StartsWith("0<"))
                {
                    type  = UnishCommandTokenType.RedirectIn;
                    token = t.Length == 2 ? "" : t.Substring(2);
                }
                else if (t.StartsWith(">>"))
                {
                    type  = UnishCommandTokenType.RedirectOutAppend;
                    token = t.Length == 2 ? "" : t.Substring(2);
                }
                else if (t.StartsWith("1>>"))
                {
                    type  = UnishCommandTokenType.RedirectOutAppend;
                    token = t.Length == 3 ? "" : t.Substring(3);
                }
                else if (t.StartsWith(">"))
                {
                    type  = UnishCommandTokenType.RedirectOut;
                    token = t.Length == 1 ? "" : t.Substring(1);
                }
                else if (t.StartsWith("1>"))
                {
                    type  = UnishCommandTokenType.RedirectOut;
                    token = t.Length == 2 ? "" : t.Substring(2);
                }
                else if (t.StartsWith("2>>"))
                {
                    type  = UnishCommandTokenType.RedirectErrAppend;
                    token = t.Length == 3 ? "" : t.Substring(3);
                }
                else if (t.StartsWith("2>"))
                {
                    type  = UnishCommandTokenType.RedirectErr;
                    token = t.Length == 2 ? "" : t.Substring(2);
                }
                else
                {
                    type  = UnishCommandTokenType.Param;
                    token = RemoveQuotesIfExist(t);
                }

                return (token, type);
            }).ToList();
            for (var i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].type)
                {
                    case UnishCommandTokenType.RedirectIn:
                    case UnishCommandTokenType.RedirectOut:
                    case UnishCommandTokenType.RedirectErr:
                    case UnishCommandTokenType.RedirectOutAppend:
                    case UnishCommandTokenType.RedirectErrAppend:
                        if (string.IsNullOrEmpty(tokens[i].token))
                        {
                            if (i + 1 < tokens.Count && tokens[i + 1].type == UnishCommandTokenType.Param)
                            {
                                tokens[i] = (tokens[i + 1].token, tokens[i].type);
                                tokens.RemoveAt(i+1);
                            }
                        }
                        break;
                }
            }

            return tokens;
        }


        public static List<(string token, bool isOption)> SplitCommand(string input)
        {
            var ret = new List<string>();
            SplitCommandInternal(input, ret);

            return ret.Select(t => (token: RemoveQuotesIfExist(t), isOption: t[0] == '-'))
                .ToList();
        }

        private static void SplitCommandInternal(string input, List<string> leading)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            var tokenStartedIndex       = -1;
            var isInStringSingle        = false;
            var isInStringDouble        = false;
            var firstInStringSpaceIndex = -1;

            var i = 0;
            for (; i < input.Length; i++)
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
                    case var space when char.IsWhiteSpace(space):
                        if (!isInStringSingle && !isInStringDouble)
                        {
                            if (tokenStartedIndex >= 0)
                            {
                                var token = input.Substring(tokenStartedIndex, i - tokenStartedIndex);
                                leading.Add(token);
                                SplitCommandInternal(input.Substring(i + 1), leading);
                                return;
                            }
                        }
                        else if (firstInStringSpaceIndex < 0)
                        {
                            firstInStringSpaceIndex = i;
                        }

                        break;
                }

                if (!char.IsWhiteSpace(c))
                {
                    if (tokenStartedIndex < 0)
                    {
                        tokenStartedIndex = i;
                    }
                }
            }

            if ((!isInStringDouble && !isInStringSingle) || firstInStringSpaceIndex < 0)
            {
                leading.Add(input.Substring(tokenStartedIndex));
            }
            else
            {
                leading.Add(input.Substring(tokenStartedIndex, firstInStringSpaceIndex - tokenStartedIndex));
                SplitCommandInternal(input.Substring(firstInStringSpaceIndex + 1), leading);
            }
        }

        public static bool TryParseSetVarExpr(string input, out string varname, out string value)
        {
            var eqIdx = input.IndexOf('=');
            if (eqIdx > 0 && eqIdx < input.Length - 1)
            {
                varname = input.Substring(0, eqIdx);
                value   = input.Substring(eqIdx + 1);
                value   = RemoveQuotesIfExist(value);
                return true;
            }

            varname = value = null;
            return false;
        }
    }
}
