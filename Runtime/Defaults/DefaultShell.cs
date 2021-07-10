﻿namespace RUtil.Debug.Shell
{
    public sealed class DefaultShell : UnishCore
    {
        public override IUnishIO            IO            { get; }
        public override IUnishCommandRunner CommandRunner { get; }
        public override IUnishDirectoryRoot Directory     { get; }

        public DefaultShell()
        {
            IO            = new DefaultUnishIO();
            CommandRunner = new DefaultCommandRunner();
            Directory     = new DefaultDirectoryRoot();
        }
    }
}
