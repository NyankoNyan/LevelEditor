
using LevelView;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    public interface IListFacade
    {
        ITextConnector AddElement(string text);
    }

    public interface ITextConnector
    {
        void Remove();

        string Text { get; set; }

    }

    public class ListField : MonoBehaviour, IListFacade
    {
        [SerializeField] LevelView.ListUI _listUI;
        private ListUIRectiveWrapper _wrapper;

        private void Awake()
        {
            Assert.IsNotNull( _listUI );

            _wrapper = new LevelView.ListUIRectiveWrapper( _listUI );
        }

        public ITextConnector AddElement(string text)
        {
            return _wrapper.AddItem( text );
        }
    }
}
