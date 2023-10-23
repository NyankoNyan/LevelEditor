using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public interface IButton
    {
        string Id { get; }
        string Text { get; set; }
        UnityAction<string> onClick { get; set; }
    }

    public interface IInteractive
    {
        bool IsAvailable { get; }
        void SetFocus(bool active);
        void MindActivate(bool active);
    }

    public class Button : MonoBehaviour, IButton, IInteractive
    {
        [SerializeField] TextMeshProUGUI _textUI;
        [SerializeField] string _id;
        [SerializeField] XRBaseInteractable _XRInteractable;
        [SerializeField] UnityEngine.UI.Button _button;

        public string Id => _id;

        public string Text
        {
            get => _textUI.text;
            set {
#if UNITY_EDITOR
                if (_textUI.text != value) {
                    UnityEditor.EditorUtility.SetDirty( _textUI );
                    UnityEditor.EditorUtility.SetDirty( transform );
                }
#endif
                _textUI.text = value;
                transform.name = $"Button {_textUI.text}";
            }
        }

        public bool IsAvailable => throw new System.NotImplementedException();

        public UnityAction<string> onClick { get; set; }

        void Awake()
        {
            if (!_textUI) {
                Debug.LogError( $"Missing inner element", this );
            }
        }

        void Start()
        {
            if (_XRInteractable) {
                _XRInteractable.selectEntered.AddListener( OnSelectEntered );
            }
            if (_button) {
                _button.onClick.AddListener( OnButtonClick );
            }
        }

        void OnDestroy()
        {
            if (_XRInteractable) {
                _XRInteractable.selectEntered.RemoveListener( OnSelectEntered );
            }
            if (_button) {
                _button.onClick.RemoveListener( OnButtonClick );
            }
        }

        public void MindActivate(bool active)
        {
            throw new System.NotImplementedException();
        }

        public void SetFocus(bool active)
        {
            throw new System.NotImplementedException();
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            onClick?.Invoke( _id );
        }

        void OnButtonClick()
        {
            onClick?.Invoke( _id );
        }
    }
}
