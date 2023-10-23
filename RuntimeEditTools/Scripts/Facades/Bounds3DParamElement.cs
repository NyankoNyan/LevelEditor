using Level;
using LevelView;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public class Bounds3DParamElement : MonoBehaviour, IParamElement
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private V3ParamElement _minComponent;
        [SerializeField] private V3ParamElement _maxComponent;
        private bool _readOnly;

        public UnityAction<int> OnComponentSelect;

        public string Name { set => _name.text = value; }

        public object Value
        {
            set {
                GridBoundsRect bounds = (GridBoundsRect)value;
                if (_minComponent) {
                    _minComponent.Value = bounds.chunkFrom;
                }
                if (_maxComponent) {
                    _maxComponent.Value = bounds.chunkTo;
                }
            }
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
            if (_minComponent) {
                _minComponent.OnComponentSelect += OnMinElemSelect;
            }
            if (_maxComponent) {
                _maxComponent.OnComponentSelect += OnMaxElemSelect;
            }
        }

        private void OnDestroy()
        {
            if (_minComponent) {
                _minComponent.OnComponentSelect -= OnMinElemSelect;
            }
            if (_maxComponent) {
                _maxComponent.OnComponentSelect -= OnMaxElemSelect;
            }
        }

        public void ClearSelection()
        {
            if (_minComponent) {
                _minComponent.ClearSelection();
            }
            if (_maxComponent) {
                _maxComponent.ClearSelection();
            }
        }

        private void OnMinElemSelect(int subpartIndex)
        {
            if (_maxComponent) {
                _maxComponent.ClearSelection();
            }
            OnComponentSelect?.Invoke( subpartIndex );
        }

        private void OnMaxElemSelect(int subpartIndex)
        {
            if (_minComponent) {
                _minComponent.ClearSelection();
            }
            OnComponentSelect?.Invoke( subpartIndex + 3 );
        }
    }
}