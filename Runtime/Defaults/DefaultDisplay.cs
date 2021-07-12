using UnityEngine;
using UnityEngine.UI;

namespace RUtil.Debug.Shell
{
    public class DefaultDisplay : MonoBehaviour
    {
        [field: SerializeField]
        public Image Background { get; private set; }

        [field: SerializeField]
        public Text Text { get; private set; }
    }
}
