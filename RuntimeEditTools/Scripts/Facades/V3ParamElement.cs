using LevelView;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public class V3ParamElement : MonoBehaviour, IParamElement
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private StringParamElement _xComponent;
        [SerializeField] private StringParamElement _yComponent;
        [SerializeField] private StringParamElement _zComponent;
        [SerializeField] private bool _isReal;
        private bool _readOnly;

        public UnityAction<int> OnComponentSelect;

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
                if (_isReal) {
                    var val = (Vector3)value;
                    if (_xComponent) {
                        _xComponent.Value = val.x.ToString( "0.00" );
                    }
                    if (_yComponent) {
                        _yComponent.Value = val.y.ToString( "0.00" );
                    }
                    if (_zComponent) {
                        _zComponent.Value = val.z.ToString( "0.00" );
                    }
                } else {
                    var val = (Vector3Int)value;
                    if (_xComponent) {
                        _xComponent.Value = val.x.ToString();
                    }
                    if (_yComponent) {
                        _yComponent.Value = val.y.ToString();
                    }
                    if (_zComponent) {
                        _zComponent.Value = val.z.ToString();
                    }
                }
            }
        }

        private void OnXClick(ListUIElement uielem)
        {
            ClearSelection();
            uielem.Selected = true;
            OnComponentSelect?.Invoke( 0 );
        }

        private void OnYClick(ListUIElement uielem)
        {
            ClearSelection();
            uielem.Selected = true;
            OnComponentSelect?.Invoke( 1 );
        }

        private void OnZClick(ListUIElement uielem)
        {
            ClearSelection();
            uielem.Selected = true;
            OnComponentSelect?.Invoke( 2 );
        }

        public bool ReadOnly { set => _readOnly = value; }

        public bool Selected
        {
            set {
                if (!value) {
                    ClearSelection();
                }
            }
        }

        private void Start()
        {
            if (_xComponent) {
                var uielem = _xComponent.GetComponent<ListUIElement>();
                uielem.onClick += OnXClick;
            }
            if (_yComponent) {
                var uielem = _yComponent.GetComponent<ListUIElement>();
                uielem.onClick += OnYClick;
            }
            if (_zComponent) {
                var uielem = _zComponent.GetComponent<ListUIElement>();
                uielem.onClick += OnZClick;
            }
        }

        private void OnDestroy()
        {
            if (_xComponent) {
                var uielem = _xComponent.GetComponent<ListUIElement>();
                uielem.onClick -= OnXClick;
            }
            if (_yComponent) {
                var uielem = _yComponent.GetComponent<ListUIElement>();
                uielem.onClick -= OnYClick;
            }
            if (_zComponent) {
                var uielem = _zComponent.GetComponent<ListUIElement>();
                uielem.onClick -= OnZClick;
            }
        }

        public void ClearSelection()
        {
            ClearSelectionComp( _xComponent );
            ClearSelectionComp( _yComponent );
            ClearSelectionComp( _zComponent );
        }

        private void ClearSelectionComp(StringParamElement component)
        {
            if (component) {
                var elem = component.GetComponent<ListUIElement>();
                if (elem.Selected) {
                    elem.Selected = false;
                }
            }
        }
    }
}