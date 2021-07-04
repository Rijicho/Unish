using System;

namespace RUtil.Debug.Shell
{
    public interface IUnishInputHandler
    {
        event Action<char> OnTextInput;

        void Initialize();
        void Update();
        bool CheckInputOnThisFrame(UnishInputType input);
    }
}