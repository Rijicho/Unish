namespace RUtil.Debug.Shell
{
    public class DefaultShell : UnishCore
    {
        public override IUnishView View { get; } = DefaultUnishView.Instance;
        public override IUnishCommandRepository CommandRepository { get; } = DefaultUnishCommandRepository.Instance;
        public override IColorParser ColorParser { get; } = DefaultColorParser.Instance;

        public override IUnishInputHandler InputHandler { get; } =
            new DefaultUnishInputHandler(DefaultTimeProvider.Instance);

        public override ITimeProvider TimeProvider { get; } = DefaultTimeProvider.Instance;
    }
}