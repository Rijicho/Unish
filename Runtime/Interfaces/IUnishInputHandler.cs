using System;

namespace RUtil.Debug.Shell
{
    public interface IUnishInputHandler
    {
        event Action<char> OnTextInput;

        void Initialize();
        void Quit();
        bool CheckInputOnThisFrame(UnishInputType input);

        char CurrentCharInput { get; }
    }
}
