using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public class UnishCore : IUniShell
    {
        // ----------------------------------
        // properties
        // ----------------------------------
        public UnishEnvSet          Env         { get; set; }
        public UnishIOs             IO          { get; set; }
        public IUnishInterpreter    Interpreter { get; set; }
        public IUnishFileSystemRoot Directory   { get; set; }
        public IUnishProcess        Parent      { get; set; }

        // ----------------------------------
        // public methods
        // ----------------------------------

        public async UniTask RunAsync()
        {
            await foreach (var input in IO.In(Parent is null))
            {
                if (Env.BuiltIn.Get(UnishBuiltInEnvKeys.Quit, false))
                {
                    return;
                }

                await Interpreter.RunCommandAsync(this, input);

                if (Env.BuiltIn.Get(UnishBuiltInEnvKeys.Quit, false))
                {
                    return;
                }
            }
        }

        public IUnishProcess Fork(UnishIOs io)
        {
            return new UnishCore
            {
                Env         = Env.Fork(),
                IO          = io,
                Interpreter = Interpreter,
                Directory   = Directory,
                Parent      = this,
            };
        }
    }
}
