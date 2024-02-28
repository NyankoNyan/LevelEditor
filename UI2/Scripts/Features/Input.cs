using TMPro;

using UnityEngine;
using UnityEngine.Assertions;

namespace UI2.Feature
{
    public class Input : IFacadeFeature
    {
        private readonly TMP_InputField _inputField;
        private IElementInstance _elem;
        private ValidateType _validateType;

        public Input(TMP_InputField inputField)
        {
            Assert.IsNotNull(inputField);
            _inputField = inputField;
        }

        public void Enable()
        {
            _inputField.onValidateInput += OnValidateInput;
            _inputField.onDeselect.AddListener(OnDeselect);

            if (!string.IsNullOrWhiteSpace(_elem.Proto.UsedState)) {
                var state = _elem.State(_elem.Proto.UsedState);
                _inputField.text = state.Get<string>();
            }
        }

        private void OnDeselect(string value)
        {
            if (!string.IsNullOrWhiteSpace(_elem.Proto.UsedState)) {
                var state = _elem.State(_elem.Proto.UsedState);
                state.Set(value);
            }
        }

        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (_validateType == ValidateType.Number) {
                if (char.IsDigit(addedChar)) {
                    return addedChar;
                } else {
                    return (char)0;
                }
            } else {
                return addedChar;
            }
        }

        public void Disable()
        {
            if (!_inputField) {
                return;
            }
            _inputField.onValidateInput -= OnValidateInput;
            _inputField.onDeselect.RemoveListener(OnDeselect);
        }

        public void Init(GameObject go, IElementInstance instance)
        {
            _elem = instance;
        }

        public void Use(string action, params object[] actonParams)
            => throw new ElementWorkflowException($"usage not available for [{nameof(Input)}] feature");

        public void Number(int maxLen = 0)
        {
            if (maxLen >= 0) {
                _inputField.characterLimit = maxLen;
            }
            _validateType = ValidateType.Number;
        }

        private enum ValidateType
        {
            None,
            Number
        }
    }
}