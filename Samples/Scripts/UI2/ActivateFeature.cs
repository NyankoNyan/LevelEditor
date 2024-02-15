using UnityEngine;
using UnityEngine.UI;

namespace UI2
{
    public class ActivateFeature : IFacadeFeature
    {
        private readonly Button _button;

        public ActivateFeature(Button button = null)
        {
            _button = button;

        }

        public void Init(GameObject go, IElementInstance instance)
        {
            
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
                _button.enabled = true;
            }
        }

        public void Deactivate()
        {
            if (_button) {
                _button.enabled = false;
            }
        }
    }
}