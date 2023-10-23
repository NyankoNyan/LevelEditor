using LevelView;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    internal class BlockProtoMenuContoller
    {
        private BlockProtoEditController _blockProtoEditController;
        private BlockListContoller _blockListContoller;

        public UnityAction onBack;

        public BlockProtoMenuContoller(BlockProtoEditController blockProtoEditController, BlockListContoller blockListContoller)
        {
            Assert.IsNotNull( blockProtoEditController );
            Assert.IsNotNull( blockListContoller );

            _blockProtoEditController = blockProtoEditController;
            _blockListContoller = blockListContoller;

            _blockListContoller.onStartEdit += OnStartEdit;
            _blockProtoEditController.onBack += OnEndEdit;
        }

        private void OnStartEdit(uint blockId)
        {
            _blockProtoEditController.Show( true );
            _blockListContoller.Show( false );
            _blockProtoEditController.SetBlockId( blockId );
        }

        private void OnEndEdit()
        {
            _blockProtoEditController.SetBlockId( 0 );
            _blockProtoEditController.Show( false );
            _blockListContoller.Show( true );
        }

        public void Init(bool addBackButton = false)
        {
            _blockProtoEditController.Init();

            _blockProtoEditController.Show( false );
            _blockListContoller.Show( false );

            if (addBackButton) {
                _blockListContoller.AddControllButton( "BACK", "BACK" );
            }

            _blockListContoller.onListAction += OnListAction;
        }

        private void OnListAction(string command, IDataContainer[] dataContainers)
        {
            if (command == "BACK") {
                onBack?.Invoke();
            }
        }

        public void Destroy()
        {
            _blockListContoller.onStartEdit -= OnStartEdit;
            _blockProtoEditController.onBack -= OnEndEdit;

            _blockListContoller.onListAction -= OnListAction;

            _blockProtoEditController.Destroy();
            _blockListContoller.Destroy();
        }

        public void Show(bool show)
        {
            _blockListContoller.Show( show );
        }
    }

    internal class GridSettingsMenuController
    {
        private GridSettingsEditController _editController;
        private GridSettingsListController _listController;

        public UnityAction onBack;

        public GridSettingsMenuController(GridSettingsEditController editController, GridSettingsListController listController)
        {
            Assert.IsNotNull( listController );
            Assert.IsNotNull( listController );

            _editController = editController;
            _listController = listController;

            _listController.onStartEdit += OnStartEdit;
            _editController.onBack += OnEndEdit;
        }

        private void OnStartEdit(uint gridSettingsId)
        {
            _editController.Show( true );
            _listController.Show( false );
            _editController.SetGridSettingsId( gridSettingsId );
        }

        private void OnEndEdit()
        {
            _editController.SetGridSettingsId( 0 );
            _editController.Show( false );
            _listController.Show( true );
        }

        private void OnListAction(string command, IDataContainer[] dataContainers)
        {
            if (command == "BACK") {
                onBack?.Invoke();
            }
        }

        public void Init(bool addBackButton = false)
        {
            _editController.Init();
            _editController.Show( false );
            _listController.Show( false );

            if (addBackButton) {
                _listController.AddControllButton( "BACK", "BACK" );
            }

            _listController.onListAction += OnListAction;
        }

        public void Destroy()
        {
            _listController.onStartEdit -= OnStartEdit;
            _editController.onBack -= OnEndEdit;

            _listController.onListAction -= OnListAction;

            _editController.Destroy();
            _listController.Destroy();
        }

        public void Show(bool show)
        {
            _listController.Show( show );
        }
    }
}