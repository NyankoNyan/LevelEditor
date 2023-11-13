using LevelView;
using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    [DefaultExecutionOrder( 2 )]
    public class RuntimeEditMenuInitializer : MonoBehaviour
    {
        [SerializeField] private MainEditorMenuFacade _mainMenu;
        [SerializeField] private Keyboard _keyboard;
        [SerializeField] private InteractiveListFacade _blockListFacade;
        [SerializeField] private ParametersListFacade _blockProtoFacade;
        [SerializeField] private InteractiveListFacade _gridSettingsListFacade;
        [SerializeField] private ParametersListFacade _gridSettingsFacade;

        private MainMenuController _mainMenuController;

        private void Awake()
        {
            Assert.IsNotNull( _mainMenu );
            Assert.IsNotNull( _blockListFacade );
            Assert.IsNotNull( _blockProtoFacade );
            Assert.IsNotNull( _keyboard );
        }

        private void Start()
        {
            var levelAPI = LevelStorage.Instance.API;

            // Block proto editor
            var blockProtoEditController = new BlockProtoEditController( levelAPI.BlockProto, _blockProtoFacade, _keyboard );

            // Block proto list
            var blockListContoller = new BlockListContoller( _blockListFacade, levelAPI.BlockProto );

            var blockProtoMenuContoller = new BlockProtoMenuContoller( blockProtoEditController, blockListContoller );

            // Grid settings editor
            var gridSettingsEditController = new GridSettingsEditController( levelAPI.GridSettingsCollection, _gridSettingsFacade, _keyboard );

            // Grid settings list
            var gridSettingsListController = new GridSettingsListController( _gridSettingsListFacade, levelAPI.GridSettingsCollection );

            var gridSettingsMenuController = new GridSettingsMenuController( gridSettingsEditController, gridSettingsListController );

            // Main organizer

            _mainMenuController = new MainMenuController( _mainMenu, blockProtoMenuContoller, gridSettingsMenuController );
            _mainMenuController.Init();
        }

        private void OnDestroy()
        {
            _mainMenuController.Destroy();
        }
    }
}