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

        void Awake()
        {
            if (!_button) {
                throw new Exception("Missing button link");
            }
        }
        void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        void OnClick()
        {
            this.ElementInstance.Click();
        }
    }
}