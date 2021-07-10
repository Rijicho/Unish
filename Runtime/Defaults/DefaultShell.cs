namespace RUtil.Debug.Shell
{
    public sealed class DefaultShell : UnishCore
    {
        public override IUnishIO                IO                { get; }
        public override IUnishCommandRepository CommandRepository { get; }
        public override IUnishCommandRunner     CommandRunner     { get; }
        public override IUnishColorParser       ColorParser       { get; }
        public override IUnishDirectoryRoot     Directory         { get; }

        public DefaultShell()
        {
            CommandRepository = DefaultUnishCommandRepository.Instance;
            CommandRunner     = new DefaultCommandRunner();
            ColorParser       = DefaultColorParser.Instance;
            IO                = new DefaultUnishIO();
            Directory         = new DefaultDirectoryRoot();
        }
    }
}
