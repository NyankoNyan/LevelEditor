using LevelView;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    public class BoolParamElement : MonoBehaviour, IParamElement
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private GameObject _trueState;
        [SerializeField] private GameObject _falseState;
        private bool _readOnly;
        private ListUIElement _uiElem;

        public string Name
        {
            set {
                if (_name) {
                    _name.text = value;
                }
            }
        }

        public bool State
        {
            set {
                if (_trueState) {
                    _trueState.SetActive( value );
                }
                if (_falseState) {
                    _falseState.SetActive( !value );
                }
            }
        }

        public object Value
        {
            set {
                bool val = (bool)value;
                if (_trueState) {
                    _trueState.SetActive( val );
                }
                if (_falseState) {
                    _falseState.SetActive( !val );
                }
            }
        }

        public bool ReadOnly
        {
            set => _readOnly = value;
        }

        public bool Selected
        {
            set => _uiElem.Selected = value;
        }

        private void Awake()
        {
            _uiElem = GetComponent<ListUIElement>();
            Assert.IsNotNull( _uiElem );
        }
    }
}