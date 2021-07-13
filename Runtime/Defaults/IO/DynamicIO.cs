using System;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public delegate UniTask<string> UnishStdIn(bool withPrompt);

    public delegate UniTask UnishStdOut(string text);

    public delegate UniTask UnishStdErr(Exception error);

    public class DynamicIO : IUnishIO
    {
        public IUnishEnv   BuiltInEnv { get; set; }
        public UnishStdIn  In         { get; private set; }
        public UnishStdOut Out        { get; private set; }
        public UnishStdErr Err        { get; private set; }

        public DynamicIO(UnishStdIn stdin, UnishStdOut stdout, UnishStdErr stderr)
        {
            In  = stdin;
            Out = stdout;
            Err = stderr;
        }

        public UniTask InitializeAsync()
        {
            return default;
        }

        public UniTask FinalizeAsync()
        {
            In  = null;
            Out = null;
            Err = null;
            return default;
        }

        public UniTask<string> ReadAsync(bool withPrompt = false)
        {
            return In(withPrompt);
        }

        public UniTask WriteAsync(string text)
        {
            return Out(text);
        }

        public UniTask WriteErrorAsync(Exception error)
        {
            return Err(error);
        }

        public event Action OnHaltInput;

        public void Redirect(UnishStdIn input)
        {
            In = input;
        }

        public void Redirect(UnishStdOut output)
        {
            Out = output;
        }

        public void Redirect(UnishStdErr error)
        {
            Err = error;
        }
    }
}
