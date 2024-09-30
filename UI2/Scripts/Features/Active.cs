using UnityEngine;
using UnityEngine.UI;

namespace UI2.Feature
{
    public class Active : IFacadeFeature
    {
        private readonly Button _button;

        public Active(Button button = null)
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
                    throw new ElementWorkflowException($"incorrect usage of [{action}] in [{nameof(Active)}] feature");
            }
        }

        public void Activate(bool active = true)
        {
            if (_button) {
                _button.enabled = active;
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