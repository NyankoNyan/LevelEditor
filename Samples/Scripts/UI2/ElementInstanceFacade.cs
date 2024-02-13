using System;

using UnityEngine;
using UnityEngine.UI;

namespace UI2
{
    public class ElementInstanceFacade : MonoBehaviour
    {
        [SerializeField] private Transform _subZone;

        public IElementInstance ElementInstance { get; internal set; }

        public Transform SubZone => _subZone ?? transform;
    }

    public class ButtonFacade : ElementInstanceFacade
    {
        [SerializeField] private Button _button;

        private void Awake()
        {
            if (!_button) {
                throw new Exception("Missing button link");
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            //this.ElementInstance.Click();
        }
    }
}