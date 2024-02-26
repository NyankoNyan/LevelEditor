using UnityEngine;
using UnityEngine.UI;

namespace UI2
{
    public class ClickFeature : IFacadeFeature
    {
        private readonly Button _button;
        private IElementInstance _instance;

        public ClickFeature(Button button)
        {
            _button = button;
            if (!_button) {
                throw new ElementWorkflowException();
            }
        }

        public void Use(string action, params object[] actonParams)
            => throw new ElementWorkflowException();

        public void Init(GameObject go, IElementInstance instance)
        {
            _instance = instance;
        }

        public void Enable()
        {
            _button.onClick.AddListener(OnClick);
        }

        public void Disable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _instance.SendFacadeSignal(Facade.Click);
        }
    }
}