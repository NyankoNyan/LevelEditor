using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    internal class MainEditorMenuFacade : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button _blockProtosButton;
        [SerializeField] private UnityEngine.UI.Button _gridSettingsButton;
        [SerializeField] private UnityEngine.UI.Button _gridStatesButton;

        public UnityAction onBlockProtosClick;
        public UnityAction onGridSettingsClick;
        public UnityAction onGridStatesClick;

        private void Start()
        {
            _blockProtosButton.onClick.AddListener( () => onBlockProtosClick?.Invoke() );
            _gridSettingsButton.onClick.AddListener( () => onGridSettingsClick?.Invoke() );
            _gridStatesButton.onClick.AddListener( () => onGridStatesClick?.Invoke() );
        }

        private void OnDestroy()
        {
            _blockProtosButton.onClick.RemoveAllListeners();
            _gridSettingsButton.onClick.RemoveAllListeners();
            _gridStatesButton.onClick.RemoveAllListeners();
        }
    }
}