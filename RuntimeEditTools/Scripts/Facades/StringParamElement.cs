using LevelView;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    public class StringParamElement : MonoBehaviour, IParamElement
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _value;
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

        public object Value
        {
            set {
                if (_value) {
                    _value.text = (string)value;
                }
            }
        }

        public bool ReadOnly { set => _readOnly = value; }
        public bool Selected { set => _uiElem.Selected = value; }

        void Awake()
        {
            _uiElem = GetComponent<ListUIElement>();
            Assert.IsNotNull( _uiElem );
        }
    }
}