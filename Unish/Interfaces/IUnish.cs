using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnish
    {
        IUnishView View { get; }

        IUnishCommandRepository CommandRepository { get; }

        IColorParser ColorParser { get; }

        IUnishInputHandler InputHandler { get; }

        ITimeProvider TimeProvider { get; }

        string Prompt { get; set; }

        UniTask RunCommandAsync(string cmd);

        void WriteLine(string line);

        UniTask<string> ReadLineAsync();

        UniTask OpenAsync();

        UniTask CloseAsync();
    }
}