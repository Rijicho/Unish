﻿using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishPresenter
    {
        IUnishIO                IO                { get; }
        IUnishCommandRepository CommandRepository { get; }
        IUnishCommandRunner     CommandRunner     { get; }
        IUnishColorParser       ColorParser       { get; }
        IUnishDirectoryRoot     Directory         { get; }
        string                  Prompt            { set; }

        UniTask RunAsync();
        void Halt();
    }
}
