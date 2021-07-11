using UnityEngine;

namespace RUtil.Debug.Shell
{
    public interface IUnishColorParser
    {
        Color Parse(string str);
        bool TryParse(string str, out Color value);
        string ColorToCode(Color color);
    }
}
