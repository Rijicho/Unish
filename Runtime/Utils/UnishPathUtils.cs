using System.Collections.Generic;
using System.Text;
using static RUtil.Debug.Shell.UnishPathConstants;

namespace RUtil.Debug.Shell
{
    public static class UnishPathUtils
    {
        public static string ConvertToHomeRelativePath(
            string input,
            string currentHomeName,
            string currentHomeRelativePath,
            string currentParentHomeRelativePath,
            out string nextHomeName)
        {
            // カレントディレクトリがRootの場合：
            var isInRoot = string.IsNullOrEmpty(currentHomeName) || currentHomeName == Root;
            if (isInRoot)
            {
                currentHomeName               = "";
                currentHomeRelativePath       = null;
                currentParentHomeRelativePath = null;
            }

            if (input == Root)
            {
                nextHomeName = "";
                return null;
            }

            // 最後のセパレータは削除
            if (input[input.Length - 1] == Separator)
            {
                input = input.Substring(0, input.Length - 1);
            }

            switch (input)
            {
                case CurrentDir:
                    nextHomeName = currentHomeName;
                    return currentHomeRelativePath;
                case ParentDir:
                    {
                        nextHomeName = currentParentHomeRelativePath == null ? "" : currentHomeName;
                        return currentParentHomeRelativePath;
                    }
                case Home:
                    nextHomeName = currentHomeName;
                    return isInRoot ? null : "";
            }

            //Rootから見たパスに変換
            if (input.StartsWith(Root))
            {
                input = input.Substring(Root.Length);
            }
            else
            {
                if (input.StartsWith(HomeRelativePrefix))
                {
                    // ~/hoge
                    if (isInRoot)
                    {
                        // hoge
                        input = input.Substring(HomeRelativePrefix.Length);
                    }
                    else
                    {
                        // home/hoge
                        input = currentHomeName + input.Substring(Home.Length);
                    }
                }
                else if (input.StartsWith(CurrentRelativePrefix))
                {
                    // ./hoge
                    if (isInRoot)
                    {
                        // hoge
                        input = input.Substring(CurrentRelativePrefix.Length);
                    }
                    else
                    {
                        // home/c/u/r/r/e/n/t/hoge
                        input = currentHomeName + currentHomeRelativePath + input.Substring(CurrentDir.Length);
                    }
                }
                else if (input.StartsWith(ParentRelativePrefix))
                {
                    // ../hoge
                    if (isInRoot || currentParentHomeRelativePath == null)
                    {
                        // hoge
                        input = input.Substring(ParentRelativePrefix.Length);
                    }
                    else
                    {
                        // home/p/a/r/e/n/t/hoge
                        input = currentHomeName + currentParentHomeRelativePath + input.Substring(ParentDir.Length);
                    }
                }
                else
                {
                    // hoge/fuga
                    if (!isInRoot)
                    {
                        // home/c/u/r/r/e/n/t/hoge/fuga
                        input = currentHomeName + currentHomeRelativePath + Separator + input;
                    }
                }
            }

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
                        pathStack.Push(currentHomeName);
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
                nextHomeName = null;
                return null;
            }

            // Root以外
            var sb = new StringBuilder();
            while (pathStack.Count > 1)
            {
                sb.Insert(0, pathStack.Pop());
                sb.Insert(0, Separator);
            }

            nextHomeName = pathStack.Pop();
            return sb.ToString();
        }
    }
}
