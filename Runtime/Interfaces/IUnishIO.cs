using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public interface IUnishIO : IUnishResource
    {
        int   HorizontalCharCount { get; }
        int   MaxLineCount        { get; }
        Color DisplayTextColor    { get; set; }
        Color BackgroundColor     { get; set; }
        UniTask<string> ReadAsync();
        UniTask WriteAsync(string text);
        UniTask WriteErrorAsync(Exception error);
    }
}
