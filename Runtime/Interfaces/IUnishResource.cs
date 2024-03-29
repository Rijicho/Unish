﻿using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishResource
    {
        UniTask InitializeAsync();
        UniTask FinalizeAsync();
    }
}
