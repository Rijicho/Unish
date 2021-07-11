namespace RUtil.Debug.Shell
{
    public sealed class DefaultShell : UnishCore
    {
        public override IUnishEnv           Env         { get; }
        public override IUnishIO            IO          { get; }
        public override IUnishInterpreter   Interpreter { get; }
        public override IUnishDirectoryRoot Directory   { get; }

        public DefaultShell()
        {
            Env         = new DefaultEnv();
            IO          = new DefaultUnishIO();
            Interpreter = new DefaultInterpreter();
            Directory   = new DefaultDirectoryRoot();
        }
    }
}
