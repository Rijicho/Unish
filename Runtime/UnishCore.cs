using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishCore : IUniShell
    {
        // ----------------------------------
        // properties
        // ----------------------------------
        public UnishEnvSet          Env         { get; }
        public UnishIOs             IO          { get; }
        public IUnishInterpreter    Interpreter { get; }
        public IUnishFileSystemRoot Directory   { get; }
        public IUnishProcess        Parent      { get; }

        // ----------------------------------
        // public methods
        // ----------------------------------
        public UnishCore(UnishEnvSet env, UnishIOs io, IUnishInterpreter interpreter, IUnishFileSystemRoot directory, IUnishProcess parent)
        {
            Env         = env;
            IO          = io;
            Interpreter = interpreter;
            Directory   = directory;
            Parent      = parent;
        }

        public async UniTask RunAsync()
        {
            await foreach (var input in IO.In(Parent is null))
            {
                if (Env.BuiltIn.Get(UnishBuiltInEnvKeys.Quit, false))
                {
                    break;
                }

                await Interpreter.RunCommandAsync(this, input);
            }
        }

        public IUnishProcess Fork(UnishIOs io)
        {
            return new UnishCore(Env.Fork(), io, Interpreter, Directory, this);
        }

        public void Halt()
        {
            Env.BuiltIn.Set(UnishBuiltInEnvKeys.Quit, true);
        }
    }
}
