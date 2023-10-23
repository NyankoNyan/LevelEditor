using LevelView;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public class MenuEventArgs
    {
        public string id;
        public IDataContainer[] dataContainers;
        public bool cancel;
    }

    public interface IMenuEventSource
    {
        delegate void MenuEventReceiverDelegate(MenuEventArgs args);
        MenuEventReceiverDelegate SendEvent { get; set; }
    }

    public class InteractiveListFacade : MonoBehaviour, IMenuEventSource
    {
        [SerializeField] List<Button> _buttons;
        [SerializeField] ListUI _listUI;
        [SerializeField] Transform _buttonGroup;
        [SerializeField] UnityEngine.UI.Button _buttonPrefab;

        public const string selectCmdId = "SELECT";
        public const string unselectCmdId = "UNSELECT";

        private IMenuEventSource.MenuEventReceiverDelegate _sendEvent;
        public IMenuEventSource.MenuEventReceiverDelegate SendEvent
        {
            get => _sendEvent;
            set => _sendEvent = value;
        }

        void Awake()
        {
            if (!_listUI) {
                throw new Exception( "Missing list" );
            }
        }

        void Start()
        {
            foreach (var button in _buttons) {
                AddButtonListeners( button );
            }
        }

        private void AddButtonListeners(Button button)
        {
            button.go.onClick.AddListener( () => {
                SendEvent?.Invoke( new MenuEventArgs() {
                    id = button.id,
                    dataContainers = _listUI.GetAllSelected().ToArray()
                } );
            } );
        }

        void OnDestroy()
        {
            foreach (var button in _buttons) {
                button.go.onClick.RemoveAllListeners();
            }
        }

        public void AddItem(string text, object data = null)
        {
            var listElem = _listUI.AddItem( text, data );
        }

        public void RemoveItem(DataFilterDelegate filter)
        {
            _listUI.RemoveItems( filter );
        }

        public void AddButton(string id, string text)
        {
            var newButton = Instantiate( _buttonPrefab, _buttonGroup );
            var textMesh = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh) {
                textMesh.text = text;
            }
            var button = new Button() {
                id = id,
                go = newButton
            };
            _buttons.Add( button );
            AddButtonListeners( button );
        }

        public void RemoveButton(string id)
        {
            Button button = _buttons.Single( x => x.id == id );
            _buttons.Remove( button );
            button.go.onClick.RemoveAllListeners();
            Destroy( button.go );
        }

        [Serializable]
        public struct Button
        {
            public UnityEngine.UI.Button go;
            public string id;
        }
    }
}
