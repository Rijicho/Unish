using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishCore : IUniShell
    {
        // ----------------------------------
        // properties
        // ----------------------------------
        public IUnishProcess       Parent      { get; }
        public IUnishEnv           Env         { get; }
        public IUnishIO            IO          { get; }
        public IUnishInterpreter   Interpreter { get; }
        public IUnishDirectoryRoot Directory   { get; }

        // ----------------------------------
        // public methods
        // ----------------------------------
        public UnishCore(IUnishEnv env, IUnishIO io, IUnishInterpreter interpreter, IUnishDirectoryRoot directory, IUnishProcess parent)
        {
            Env         = env;
            IO          = io;
            Interpreter = interpreter;
            Directory   = directory;
            Parent      = parent;
        }

        public async UniTask RunAsync()
        {
            var env    = this.GetGlobalEnv();
            var prompt = Parent is IUnishRoot;
            while (!env[UnishBuiltInEnvKeys.Quit].CastOr(false))
            {
                await Interpreter.RunCommandAsync(this, await IO.ReadAsync(prompt));
            }
        }

        public UnishCore Fork(IUnishEnv env, IUnishIO io)
        {
            return new UnishCore(env, io, Interpreter, Directory, this);
        }

        public void Halt()
        {
            this.GetGlobalEnv().Set(UnishBuiltInEnvKeys.Quit, true);
        }
    }
}
