using UnityEngine;

namespace RUtil.Debug.Shell
{
    public sealed class DefaultShell : UnishCore
    {
        public override IUnishIO                IO                { get; }
        public override IUnishCommandRepository CommandRepository { get; }
        public override IUnishCommandRunner     CommandRunner     { get; }
        public override IUnishColorParser       ColorParser       { get; }
        public override IUnishTimeProvider      TimeProvider      { get; }
        public override IUnishRcRepository      RcRepository      { get; }
        public override IUnishDirectoryRoot     Directory         { get; }

        public DefaultShell()
        {
            var inputHandler = new DefaultUnishInputHandler(DefaultTimeProvider.Instance);
            CommandRepository = DefaultUnishCommandRepository.Instance;
            CommandRunner     = new DefaultCommandRunner();
            ColorParser       = DefaultColorParser.Instance;
            TimeProvider      = DefaultTimeProvider.Instance;
            RcRepository      = DefaultUnishRcRepository.Instance;
            IO                = new DefaultUnishIO(inputHandler, TimeProvider);
            Directory = new DefaultDirectoryRoot(new[]
            {
                new RealFileSystem("pdp", Application.persistentDataPath),
                new RealFileSystem("dp", Application.dataPath),
            });
        }
    }
}
