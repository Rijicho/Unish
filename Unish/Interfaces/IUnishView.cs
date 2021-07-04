using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public interface IUnishView
    {
        int HorizontalCharCount { get; }
        int MaxLineCount { get; }
        string DisplayText { get; set; }
        Color DisplayTextColor { get; set; }
        Color BackgroundColor { get; set; }
        UniTask InitializeAsync();
        UniTask DestroyAsync();
    }
}