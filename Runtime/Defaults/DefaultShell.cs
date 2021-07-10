namespace RUtil.Debug.Shell
{
    public sealed class DefaultShell : UnishCore
    {
        public override IUnishIO            IO          { get; }
        public override IUnishInterpreter   Interpreter { get; }
        public override IUnishDirectoryRoot Directory   { get; }

        public DefaultShell()
        {
            IO          = new DefaultUnishIO();
            Interpreter = new DefaultInterpreter();
            Directory   = new DefaultDirectoryRoot();
        }
    }
}
