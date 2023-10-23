using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public interface IKeyboard
    {
        UnityAction<string> onClick { get; set; }
    }

    public class Keyboard : MonoBehaviour, IKeyboard
    {
        public const string SPACE = "Space";
        public const string BACKSPACE = "Backspace";
        public const string CAPS = "Caps";

        public UnityAction<string> onClick { get; set; }

        private List<IButton> _buttons = new();
        private bool capsMode = true;

        private void Start()
        {
            CollectButtons();
            foreach (var button in _buttons) {
                button.onClick += OnButtonClick;
            }
        }

        private void OnDestroy()
        {
            foreach (var button in _buttons) {
                button.onClick -= OnButtonClick;
            }
        }

        public void UpdateButtonsTexts()
        {
            foreach (var button in GetComponentsInChildren<IButton>()) {
                if (button.Id.Length == 1 && IsSelfExplanatory( button.Id[0] )) {
                    if (capsMode) {
                        button.Text = button.Id;
                    } else {
                        button.Text = button.Id.ToLower();
                    }
                } else if (button.Id == SPACE) {
                    button.Text = "";
                } else if (button.Id == BACKSPACE) {
                    button.Text = "\u00ab";
                } else if (button.Id == CAPS) {
                    button.Text = "CAPS";
                }
            }
        }

        private void CollectButtons()
        {
            _buttons.Clear();
            _buttons.AddRange( GetComponentsInChildren<IButton>() );
        }

        private void OnButtonClick(string buttonId)
        {
            if (buttonId == CAPS) {
                capsMode = !capsMode;
                UpdateButtonsTexts();
            } else {
                onClick?.Invoke( IdToText( buttonId ) );
            }
        }

        private bool IsSelfExplanatory(char ch)
        {
            return ( ch >= 'A' && ch <= 'Z' )
                        || ( ch >= '0' && ch <= '9' )
                        || ch == '_' || ch == '.';
        }

        private string IdToText(string buttonId)
        {
            if (buttonId.Length == 1 && IsSelfExplanatory( buttonId[0] )) {
                if (capsMode) {
                    return buttonId;
                } else {
                    return buttonId.ToLower();
                }
            } else if (buttonId == SPACE) {
                return " ";
            } else {
                return buttonId;
            }
        }
    }
}