using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishCore : IUniShell, IUnishProcess
    {
        // ----------------------------------
        // properties
        // ----------------------------------
        public IUnishProcess     Parent      { get; }
        public UnishEnvSet       Env         { get; }
        public IUnishIO          IO          { get; }
        public IUnishInterpreter Interpreter { get; }
        public IUnishFileSystemRoot  Directory   { get; }

        // ----------------------------------
        // public methods
        // ----------------------------------
        public UnishCore(UnishEnvSet env, IUnishIO io, IUnishInterpreter interpreter, IUnishFileSystemRoot directory, IUnishProcess parent)
        {
            Env         = env;
            IO          = io;
            Interpreter = interpreter;
            Directory   = directory;
            Parent      = parent;
        }

        public async UniTask RunAsync()
        {
            while (!Env.BuiltIn[UnishBuiltInEnvKeys.Quit].CastOr(false))
            {
                await Interpreter.RunCommandAsync(this, await IO.ReadAsync(Parent is null));
            }
        }

        public IUnishProcess Fork(IUnishIO io)
        {
            return new UnishCore(Env.Fork(), io, Interpreter, Directory, this);
        }

        public void Halt()
        {
            Env.BuiltIn.Set(UnishBuiltInEnvKeys.Quit, true);
        }
    }
}
