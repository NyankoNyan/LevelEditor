using RuntimeEditTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace LevelView
{
    public class ListUIRectiveWrapper
    {
        ListUI _listUI;

        public ListUIRectiveWrapper(ListUI listUI)
        {
            _listUI = listUI;
        }

        public ElementConnector AddItem(string text, object data = null)
            => new ElementConnector( _listUI.AddItem( text, data ), _listUI );

        public class ElementConnector : ITextConnector
        {
            ListUIElement _item;
            private ListUI _listUI;

            internal ElementConnector(ListUIElement item, ListUI listUI)
            {
                _item = item;
                _listUI = listUI;
            }

            public void Remove()
            {
                _listUI.RemoveItem( _item );
            }

            public string Text
            {
                get => _item.Text;
                set {
                    _item.Text = value;
                }
            }
        }
    }

    public class ListUI : MonoBehaviour
    {
        [SerializeField] ListUIElement _baseElement;
        [SerializeField] bool _multipleSelect = false;

        List<ListUIElement> _items = new();
        int _lastSelect = -1;

        private void Awake()
        {
            Assert.IsNotNull( _baseElement );
        }

        private void Start()
        {
            if (_items.Count == 0) {
                _baseElement.gameObject.SetActive( false );
            }
        }

        public void Select(int index)
        {
            if (index < 0 || index >= _items.Count) {
                return;
            }
            if (!_multipleSelect && _lastSelect >= 0) {
                Unselect( _lastSelect );
            }
            var item = _items[index];
            item.Selected = true;
            _lastSelect = index;
        }

        public void Select(ListUIElement elem)
        {
            Select( _items.IndexOf( elem ) );
        }

        public void Unselect(int index)
        {
            if (index < 0 || index >= _items.Count) {
                return;
            }
            var item = _items[index];
            item.Selected = false;
            if (_lastSelect == index) {
                _lastSelect = -1;
            }
        }

        public void Unselect(ListUIElement elem)
        {
            Unselect( _items.IndexOf( elem ) );
        }

        public void SelectNext()
        {
            if (_lastSelect >= 0 && _items.Count > 0) {
                int nextSelect = ( _lastSelect + 1 ) % _items.Count;
                Select( nextSelect );
            }
        }

        public void SelectPrevious()
        {
            if (_lastSelect >= 0 && _items.Count > 0) {
                int nextSelect = ( _items.Count + _lastSelect - 1 ) % _items.Count;
                Select( nextSelect );
            }
        }

        public ListUIElement AddItem(string text, object data = null)
        {
            ListUIElement newItem;
            if (_items.Count == 0) {
                newItem = _baseElement;
                _baseElement.gameObject.SetActive( true );
            } else {
                newItem = Instantiate( _baseElement, _baseElement.transform.parent );
            }
            newItem.Text = text;
            newItem.Data = data;

            _items.Add( newItem );

            if (_multipleSelect && _items.Count == 1) {
                Select( 0 );
            }

            newItem.onClick += (item) => {
                if (item.Selected) {
                    Unselect( item );
                } else {
                    Select( item );
                }
            };

            return newItem;
        }

        public void RemoveItem(int index)
        {
            ListUIElement elem;
            if (_items.Count == 1) {
                elem = _baseElement;
                _baseElement.gameObject.SetActive( false );
            } else {
                elem = _items[index];
                Destroy( elem.gameObject );
                if (index == 0) {
                    _baseElement = _items[1];
                }
            }

            if (elem.onClick != null) {
                foreach (Delegate d in elem.onClick.GetInvocationList()) {
                    elem.onClick -= (UnityAction<ListUIElement>)d;
                }
            }

            _items.RemoveAt( index );

            if (index < _lastSelect) {
                _lastSelect--;
            } else if (index == _lastSelect) {
                if (_multipleSelect) {
                    if (index == _items.Count) {
                        Select( _items.Count - 1 );
                    } else {
                        Select( index );
                    }
                } else {
                    _lastSelect = -1;
                }
            }
        }

        public void RemoveItem(ListUIElement item)
        {
            RemoveItem( _items.IndexOf( item ) );
        }

        public void RemoveItems(DataFilterDelegate filter)
        {
            var removeList = _items.Where( x => filter( x ) ).ToArray();
            foreach (var item in removeList) {
                RemoveItem( item );
            }
        }

        public void RemoveAll()
        {
            for (int i = _items.Count - 1; i >= 0; i--) {
                RemoveItem( i );
            }
        }

        public IEnumerable<ListUIElement> GetAllSelected()
        {
            return _items.Where( x => x.Selected );
        }

    }
}
