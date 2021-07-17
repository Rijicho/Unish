using System;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public delegate UniTask<string> UnishFdIn(bool withPrompt);

    public delegate UniTask UnishFdOut(string text);

    public delegate UniTask UnishFdErr(Exception error);

    public class UnishIOs
    {
        public IUnishEnv  BuiltInEnv { get; }
        public UnishFdIn  In         { get; }
        public UnishFdOut Out        { get; }
        public UnishFdErr Err        { get; }

        public UnishIOs(UnishFdIn stdin, UnishFdOut stdout, UnishFdErr stderr, IUnishEnv builtInEnv)
        {
            In         = stdin;
            Out        = stdout;
            Err        = stderr;
            BuiltInEnv = builtInEnv;
        }
    }
}
