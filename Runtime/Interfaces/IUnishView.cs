using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public interface IUnishView
    {
        int    HorizontalCharCount { get; }
        int    MaxLineCount        { get; }
        Color  DisplayTextColor    { get; set; }
        Color  BackgroundColor     { get; set; }
        UniTask InitializeAsync();
        UniTask DestroyAsync();
        void Write(string text);
        UniTask<string> ReadLine();
    }
}
