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
        UniTask WriteAsync(string text);
        UniTask<string> ReadAsync();
    }
}
