using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public class InputFieldLinker
    {
        public UnityAction onLinked;
        public UnityAction onUnlinked;
        public UnityAction<string> textChanged;


        private List<IKeyboard> _keyboards = new();
        private IChangeText _textFacade;


        public void Link(IChangeText textFacade)
        {
            Assert.IsNotNull( textFacade );
            if (_textFacade != null) {
                Unlink();
            }
            _textFacade = textFacade;
            _textFacade.Focus = true;
            onLinked?.Invoke();
        }

        public void Unlink()
        {
            _textFacade.Focus = false;
            _textFacade = null;
            onUnlinked?.Invoke();
        }

        public void AddKeyboard(IKeyboard keyboard)
        {
            _keyboards.Add( keyboard );
            keyboard.onClick += ReceiveKeySignal;
        }

        public void Destroy()
        {
            foreach (var keyboard in _keyboards) {
                keyboard.onClick -= ReceiveKeySignal;
            }
            _keyboards.Clear();
        }

        void ReceiveKeySignal(string id)
        {
            if (_textFacade != null) {
                string value = _textFacade.Text;

                if (id.Length == 1
                    && ( ( id[0] >= 'A' && id[0] <= 'Z' )
                        || ( id[0] >= '0' && id[0] <= '9' )
                        || id[0] == '_' )) {
                    value += id;
                } else if (id == "Space") {
                    value += ' ';
                } else if (id == "Backspace") {
                    value = value.Substring( 0, value.Length - 1 );
                }

                _textFacade.Text = value;
                textChanged?.Invoke( value );
            }
        }
    }
}
