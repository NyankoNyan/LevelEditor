using UnityEngine;

using TMPro;

namespace UI2.Feature
{
    public class MainText : IFacadeFeature
    {
        private readonly TextMeshProUGUI _textObj;

        public MainText(TextMeshProUGUI textObj)
        {
            _textObj = textObj;
        }

        public void Use(string action, params object[] actionParams)
        {
            if (action == "SET_TEXT") {
                if (actionParams.Length == 1 && actionParams[0] is string s) {
                    SetText(s);
                } else {
                    throw new ElementWorkflowException($"incorrect usage of [{action}] in [{nameof(MainText)}] feature");
                }
            } else {
                throw new ElementWorkflowException($"unknown action [{action}] in [{nameof(MainText)}] feature usage");
            }
        }

        public void Init(GameObject go, IElementInstance instance)
        {
            if (!_textObj) {
                throw new ElementWorkflowException($"missing reference to text object in [{nameof(MainText)}] feature");
            }
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void SetText(string text)
        {
            _textObj.text = text;
        }
    }
}