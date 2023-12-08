using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    internal abstract class BaseParameterEditController
    {
        public Action onBack;

        protected ParametersListFacade _facade;
        protected EditConnector _currentEditConnector;
        protected IParametersConnector _connector;

        private Keyboard _keyboard;

        public BaseParameterEditController(ParametersListFacade facade, Keyboard keyboard)
        {
            Assert.IsNotNull( facade );
            Assert.IsNotNull( keyboard );

            _facade = facade;
            _keyboard = keyboard;

            _facade.OnParameterFocused += OnParameterFocused;
            _facade.OnParameterUnfocused += OnParameterUnfocused;
        }

        public void Init()
        {
            _facade.Init( _connector );
            _facade.OnBack += OnBackReceiver;
        }

        public void Show(bool show)
        {
            _facade.gameObject.SetActive( show );
        }

        public void Destroy()
        {
            _facade.OnBack -= OnBackReceiver;

            _facade.OnParameterFocused -= OnParameterFocused;
            _facade.OnParameterUnfocused -= OnParameterUnfocused;
        }

        private void OnParameterUnfocused(string name)
        {
            if (_currentEditConnector != null) {
                _currentEditConnector = null;
                _keyboard.onClick -= OnKeyboardInput;
            }
        }

        private void OnParameterFocused(string name, EditConnector editConnector)
        {
            if (_currentEditConnector == null) {
                _keyboard.onClick += OnKeyboardInput;
            }
            _currentEditConnector = editConnector;
        }

        private void OnKeyboardInput(string key)
        {
            if (key == Keyboard.BACKSPACE) {
                string value = (string)_currentEditConnector.Value;
                if (value.Length > 0) {
                    value = value.Substring( 0, value.Length - 1 );
                }
                _currentEditConnector.Value = value;
            } else {
                string value = (string)_currentEditConnector.Value;
                value += key;
                _currentEditConnector.Value = value;
            }
        }

        private void OnBackReceiver() => onBack?.Invoke();
    }
}