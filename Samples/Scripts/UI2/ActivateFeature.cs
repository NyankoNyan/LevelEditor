using UnityEngine;
using UnityEngine.UI;

namespace UI2
{
    public class ActivateFeature : IFacadeFeature
    {
        private Button _button;

        public void Init(GameObject go, IElementInstance instance)
        {
            _button = go.GetComponent<Button>();
            if (!_button) {
                throw new ElementWorkflowException();
            }
        }

        public void Enable() { }

        public void Disable() { }

        public void Use(string action, params object[] actonParams)
        {
            switch (action) {
                case "ACTIVATE":
                    Activate();
                    break;

                case "DEACTIVATE":
                    Deactivate();
                    break;

                default:
                    throw new ElementWorkflowException();
            }
        }

        public void Activate()
        {
            if (_button) {
                _button.enable = true;
            }
        }

        public void Deactivate()
        {
            if (_button) {
                _button.enable = false;
            }
        }
    }
}