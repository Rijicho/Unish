﻿using System;
using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishIO : IUnishResourceWithEnv
    {
        UniTask<string> ReadAsync(bool withPrompt = false);
        UniTask WriteAsync(string text);
        UniTask WriteErrorAsync(Exception error);

        event Action OnHaltInput;
    }
}
