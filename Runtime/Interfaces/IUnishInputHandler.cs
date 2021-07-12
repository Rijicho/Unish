using System;

namespace RUtil.Debug.Shell
{
    public interface IUnishInputHandler : IUnishResource
    {
        bool CheckInputOnThisFrame(UnishInputType input);

        char CurrentCharInput { get; }
    }
}
