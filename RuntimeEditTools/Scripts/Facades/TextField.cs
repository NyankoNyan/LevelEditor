using UnityEngine;
using TMPro;

namespace RuntimeEditTools
{
    public interface IChangeText
    {
        string Text { get; set; }
        bool Focus { get; set; }
    }

    public class TextField : MonoBehaviour, IChangeText
    {
        [SerializeField] TextMeshProUGUI _textMesh;
        [SerializeField] int _maxTextLength = 30;

        public string Text
        {
            get => _textMesh.text;
            set {
                string val = _maxTextLength > 0 ? value.Substring( 0, Mathf.Min( _maxTextLength, value.Length ) ) : value;
                if (_textMesh.text != val) {
                    _textMesh.text = val;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty( gameObject );
#endif
                }
            }
        }

        public bool Focus { get; set; }
    }
}
