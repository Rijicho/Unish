using System.Collections.Generic;
using System.Text;
using static RUtil.Debug.Shell.UnishPathConstants;

namespace RUtil.Debug.Shell
{
    public static class UnishPathUtils
    {
        public static string GetParentPath(string path)
        {
            var lastSeparator = path.LastIndexOf(Separator);
            if (lastSeparator <= 0)
            {
                return Root;
            }

            return path.Substring(0, lastSeparator);
        }

        public static string ConvertToAbsolutePath(string input, string pwd, string homePath)
        {
            if (input == Root)
            {
                return Root;
            }

            // 最後のセパレータは削除
            input = input.TrimEnd(Separator);

            if (input == Home || input == HomeRelativePrefix)
            {
                return homePath ?? Root;
            }

            if (input == CurrentDir || input == CurrentRelativePrefix)
            {
                return pwd;
            }

            if (input == ParentDir || input == ParentRelativePrefix)
            {
                if (pwd == Root)
                {
                    return Root;
                }

                return GetParentPath(pwd);
            }

            var isInRoot = homePath == Root;

            // フルパスに変換
            if (!input.StartsWith(Root))
            {
                if (input.StartsWith(HomeRelativePrefix))
                {
                    // ~/hoge
                    if (isInRoot)
                    {
                        // /hoge
                        input = Root + input.Substring(HomeRelativePrefix.Length);
                    }
                    else
                    {
                        // /h/o/m/e/hoge
                        input = homePath + Root + input.Substring(HomeRelativePrefix.Length);
                    }
                }
                else if (input.StartsWith(CurrentRelativePrefix))
                {
                    if (pwd == Root)
                    {
                        input = Root + input.Substring(CurrentRelativePrefix.Length);
                    }
                    else
                    {
                        input = pwd + Separator + input.Substring(CurrentRelativePrefix.Length);
                    }
                }
                else if (input.StartsWith(ParentRelativePrefix))
                {
                    var parent = GetParentPath(pwd);
                    if (parent == Root)
                    {
                        input = parent + input.Substring(ParentRelativePrefix.Length);
                    }
                    else
                    {
                        input = parent + Separator + input.Substring(ParentRelativePrefix.Length);
                    }
                }
                else
                {
                    if (pwd == Root)
                    {
                        input = Root + input;
                    }
                    else
                    {
                        input = pwd + Separator + input;
                    }
                }
            }


            // 先頭のRoot記号を一旦削除
            input = input.Substring(Root.Length);

            var pathStack = new Stack<string>();

            for (int i = 0, j = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c != Separator && c != AltSeparator && i != input.Length - 1)
                {
                    continue;
                }

                var node = c == Separator || c == AltSeparator
                    ? input.Substring(j, i - j)
                    : input.Substring(j);
                switch (node)
                {
                    case CurrentDir:
                        break;
                    case ParentDir when pathStack.Count > 0:
                        pathStack.Pop();
                        break;
                    case ParentDir:
                        break;
                    case Home:
                        pathStack.Clear();
                        if (!isInRoot)
                        {
                            pathStack.Push(homePath);
                        }

                        break;
                    default:
                        pathStack.Push(node);
                        break;
                }

                j = i + 1;
            }

            // Root
            if (pathStack.Count == 0)
            {
                return Root;
            }

            // Root以外
            var sb = new StringBuilder();
            while (pathStack.Count > 0)
            {
                sb.Insert(0, pathStack.Pop());
                sb.Insert(0, Separator);
            }

            return sb.ToString();
        }

        public static IEnumerable<string> SplitPath(string path)
        {
            if (path == null || string.IsNullOrWhiteSpace(path) || path == Root)
            {
                yield break;
            }

            foreach (var entry in path.Split(Separator))
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                yield return entry.Trim();
            }
        }
    }
}
