using UnityEngine;
using UnityEngine.UI;

namespace UI2
{
    public class ClickFeature : IFacadeFeature
    {
        private Button _button;
        private IElementInstance _instance;

        public void Use(string action, params object[] actonParams)
            => throw new ElementWorkflowException();

        public void Init(GameObject go, IElementInstance instance)
        {
            _button = go.GetComponent<Button>();
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