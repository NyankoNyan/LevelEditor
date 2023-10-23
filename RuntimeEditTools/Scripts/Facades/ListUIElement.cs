using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LevelView
{
    public interface IDataContainer
    {
        public object Data { get; set; }
    }

    public delegate bool DataFilterDelegate(IDataContainer d);


    public class ListUIElement : MonoBehaviour, IDataContainer
    {
        [SerializeField] TextMeshProUGUI _textMesh;
        [SerializeField] Button _button;
        [SerializeField] Transform _selectedOverlay;

        public UnityAction<ListUIElement> onClick;

        bool _selected;


        public string Text
        {
            get {
                return _textMesh.text;
            }
            set {
                _textMesh.text = value;
            }
        }

        public object Data { get; set; }

        public bool Selected
        {
            get => _selected;
            set {
                _selected = value;
                _selectedOverlay.gameObject.SetActive( _selected );
            }
        }

        public void Start()
        {
            if (_button) {
                _button.onClick.AddListener( () => {
                    onClick?.Invoke( this );
                } );
            }
            Selected = _selected;
        }

        public void OnDestroy()
        {
            if (_button) {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}
