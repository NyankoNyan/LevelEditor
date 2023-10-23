using Level;
using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    class BlockEditorFacade : MonoBehaviour
    {
        [SerializeField] InputField _nameIdInputField;
        [SerializeField] InputField _tagInputField;
        [SerializeField] InputField _formFactorInputField;
        [SerializeField] Vector3IntDisplay _sizeInput;
        [SerializeField] Keyboard _mainKeyboard;

        private BlockEditorController _controller;
        private BlockProto _blockProto;
        private ActiveInputField _activeInputField;

        void Awake()
        {
            Assert.IsNotNull( _nameIdInputField );
            Assert.IsNotNull( _tagInputField );
            Assert.IsNotNull( _formFactorInputField );
            Assert.IsNotNull( _sizeInput );
        }

        void Start()
        {
            Init();
            //_controller = new BlockEditorController(
            //    _nameIdInputField.SelectButton,
            //    _tagInputField.SelectButton,
            //    _formFactorInputField.SelectButton,
            //    _nameIdInputField.InputText,
            //    _tagInputField.InputText,
            //    _formFactorInputField.InputText,
            //    new IKeyboard[] { _mainKeyboard } );
        }

        void Init()
        {
            InputFieldLinker inputFieldLinker = new();
            inputFieldLinker.AddKeyboard( _mainKeyboard );

            _nameIdInputField.SelectButton.onClick += (_) => {
                inputFieldLinker.Link( _nameIdInputField.InputText );
            };
            _tagInputField.SelectButton.onClick += (_) => {
                inputFieldLinker.Link( _tagInputField.InputText );
            };
            _formFactorInputField.SelectButton.onClick += (_) => {
                inputFieldLinker.Link( _formFactorInputField.InputText );
            };

            inputFieldLinker.textChanged += (text) => {
                if (_blockProto != null) {
                    switch (_activeInputField) {
                        case ActiveInputField.Name:
                            _blockProto.Name = text;
                            break;
                        case ActiveInputField.Tag:
                            _blockProto.Tag = text;
                            break;
                        case ActiveInputField.FormFactor:
                            _blockProto.FormFactor = text;
                            break;
                    }
                }
            };
        }

        public void SetEditBlock(BlockProto blockProto)
        {
            _blockProto = blockProto;
            UpdateViewFields();
        }

        private void UpdateViewFields()
        {
            if (_blockProto != null) {
                _nameIdInputField.InputText.Text = _blockProto.Name;
                _tagInputField.InputText.Text = _blockProto.Settings.layerTag;
                _formFactorInputField.InputText.Text = _blockProto.Settings.formFactor;
                _sizeInput.Value = _blockProto.Settings.size;
            } else {
                _nameIdInputField.InputText.Text = "";
                _tagInputField.InputText.Text = "";
                _formFactorInputField.InputText.Text = "";
                _sizeInput.Value = Vector3Int.zero;
            }
        }

        private enum ActiveInputField
        {
            None,
            Name,
            Tag,
            FormFactor
        }
    }
}
