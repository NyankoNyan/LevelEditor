namespace RuntimeEditTools
{
    internal class MainMenuController
    {
        private MainEditorMenuFacade _facade;
        private BlockProtoMenuContoller _blockMenuController;
        private GridSettingsMenuController _gridSettingsMenuController;

        public MainMenuController(MainEditorMenuFacade facade, BlockProtoMenuContoller blockMenuController, GridSettingsMenuController gridSettingsMenuController)
        {
            _facade = facade;
            _blockMenuController = blockMenuController;
            _gridSettingsMenuController = gridSettingsMenuController;

            _facade.onBlockProtosClick += OnBlockProtos;
            _facade.onGridSettingsClick += OnGridSettings;
            _facade.onGridStatesClick += OnGridStates;
        }

        public void Init()
        {
            _blockMenuController.Init( true );
            _gridSettingsMenuController.Init( true );

            _facade.gameObject.SetActive( true );
            _blockMenuController.Show( false );
            _gridSettingsMenuController.Show( false );

            _blockMenuController.onBack += OnBlockMenuExit;
            _gridSettingsMenuController.onBack += OnGriSettingsMenuExit;
        }

        private void OnGriSettingsMenuExit()
        {
            _facade.gameObject.SetActive( true );
            _gridSettingsMenuController.Show( false );
        }

        private void OnBlockMenuExit()
        {
            _facade.gameObject.SetActive( true );
            _blockMenuController.Show( false );
        }

        private void OnGridStates()
        {
        }

        private void OnGridSettings()
        {
            _facade.gameObject.SetActive( false );
            _gridSettingsMenuController.Show( true );
        }

        private void OnBlockProtos()
        {
            _facade.gameObject.SetActive( false );
            _blockMenuController.Show( true );
        }

        public void Destroy()
        {
            _facade.onBlockProtosClick -= OnBlockProtos;
            _facade.onGridSettingsClick -= OnGridSettings;
            _facade.onGridStatesClick -= OnGridStates;

            _blockMenuController.onBack -= OnBlockMenuExit;
            _gridSettingsMenuController.onBack -= OnGriSettingsMenuExit;

            _blockMenuController.Destroy();
            _gridSettingsMenuController.Destroy();
        }
    }
}