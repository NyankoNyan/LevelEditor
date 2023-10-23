using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    public class ArrowDisplay : MonoBehaviour
    {
        [SerializeField] TextField _textField;
        [SerializeField] Button _buttonUp;
        [SerializeField] Button _buttonDown;

        [SerializeField] int _defaultNumber = 1;
        [SerializeField] int _from = 1;
        [SerializeField] int _to = 9;

        public int Value
        {
            get => int.Parse( _textField.Text );
            set {
                string val = Mathf.Clamp( value, _from, _to ).ToString();
                if (_textField.Text != val) {
                    _textField.Text = val;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty( gameObject );
#endif
                }
            }
        }

        private void Awake()
        {
            Assert.IsNotNull( _textField );
            Assert.IsNotNull( _buttonUp );
            Assert.IsNotNull( _buttonDown );
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_textField) {
                Value = _defaultNumber;
            }
        }
#endif
    }
}
