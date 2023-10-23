using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace RuntimeEditTools
{
    public class InputField : MonoBehaviour, IChangeText
    {
        [SerializeField] TextField _textField;
        [SerializeField] TextMeshProUGUI _captionTextMesh;
        [SerializeField] Button _selectButton;
        [SerializeField] Button _errorButton;

        [SerializeField] string _defaultCaption;
        [SerializeField] string _defaultText;


        public string Text
        {
            get => _textField.Text;
            set => _textField.Text = value;
        }

        public string Caption
        {
            get
                => _captionTextMesh.text;
            set {
                if (value != _captionTextMesh.text) {
                    _captionTextMesh.text = value;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty( gameObject );
#endif
                }
            }
        }

        public bool Focus { get; set; }
        public IButton SelectButton => _selectButton;
        public IChangeText InputText => _textField;


        void Awake()
        {
            Assert.IsNotNull( _textField );
            Assert.IsNotNull( _captionTextMesh );
            Assert.IsNotNull( _selectButton );
            Assert.IsNotNull( _errorButton );
        }

        void Start()
        {
            Caption = _defaultCaption;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_captionTextMesh) {
                Caption = _defaultCaption;
            }
            if (_textField) {
                Text = _defaultText;
            }
        }
#endif


    }
}
