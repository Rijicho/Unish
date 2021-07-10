using System;

namespace RUtil.Debug.Shell
{
    public interface IUnishInputHandler : IUnishResource
    {
        event Action<char> OnTextInput;

        bool CheckInputOnThisFrame(UnishInputType input);

        char CurrentCharInput { get; }
    }
}
