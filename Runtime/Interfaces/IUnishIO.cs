using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public interface IUnishIO
    {
        int   HorizontalCharCount { get; }
        int   MaxLineCount        { get; }
        Color DisplayTextColor    { get; set; }
        Color BackgroundColor     { get; set; }
        UniTask InitializeAsync();
        UniTask DestroyAsync();
        UniTask WriteAsync(string text);
        UniTask<string> ReadAsync();
    }
}
