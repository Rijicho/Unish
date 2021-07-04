using UnityEngine;
using UnityEngine.UI;

namespace RUtil.Debug.Shell
{
    public class DefaultUnishViewRoot : MonoBehaviour
    {
        public Image Background;
        public Text Text;
        public int CharCountPerLine = 100;
        public int MaxLineCount = 24;
    }
}