using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    public class Vector3IntDisplay : MonoBehaviour
    {
        [SerializeField] ArrowDisplay _xDisplay;
        [SerializeField] ArrowDisplay _yDisplay;
        [SerializeField] ArrowDisplay _zDisplay;
        [SerializeField] TextField _captionField;

        [SerializeField] Vector3Int _defaultValue;
        [SerializeField] string _defaultCaption;

        public Vector3Int Value
        {
            get {
                return new Vector3Int( _xDisplay.Value, _yDisplay.Value, _zDisplay.Value );
            }
            set {
                _xDisplay.Value = value.x;
                _yDisplay.Value = value.y;
                _zDisplay.Value = value.z;
            }
        }

        public string Caption
        {
            get => _captionField.Text;
            set => _captionField.Text = value;
        }

        private void Awake()
        {
            Assert.IsNotNull( _xDisplay );
            Assert.IsNotNull( _yDisplay );
            Assert.IsNotNull( _zDisplay );
            Assert.IsNotNull( _captionField );
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_xDisplay && _yDisplay && _zDisplay) {
                Value = _defaultValue;
            }
            if (_captionField) {
                Caption = _defaultCaption;
            }
        }
#endif
    }
}
