using Level;
using Level.API;
using LevelView;
using System;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    internal class BlockListContoller
    {
        private const string addCmdId = "ADD";
        private const string removeCmdId = "REMOVE";
        private const string editCmdId = "EDIT";

        private InteractiveListFacade _listFacade;
        private IBlockProtoAPI _blockProtoAPI;

        public UnityAction<uint> onStartEdit;
        public UnityAction<string, IDataContainer[]> onListAction;

        public BlockListContoller(
            InteractiveListFacade listFacade,
            IBlockProtoAPI blockProtoAPI)
        {
            _listFacade = listFacade;
            _blockProtoAPI = blockProtoAPI;

            // From model to view
            _blockProtoAPI.onBlockProtoAdded += AddBlockToList;
            _blockProtoAPI.onBlockProtoRemoved += RemoveBlockFromList;

            foreach (var blockProto in blockProtoAPI.BlockProtos) {
                AddBlockToList( blockProto );
            }

            // From view to model
            _listFacade.SendEvent += HandleListEvent;
        }

        public void Destroy()
        {
            _blockProtoAPI.onBlockProtoAdded -= AddBlockToList;
            _blockProtoAPI.onBlockProtoRemoved -= RemoveBlockFromList;
            _listFacade.SendEvent -= HandleListEvent;
        }

        private void AddBlockToList(BlockProto blockProto)
        {
            _listFacade.AddItem( blockProto.Name, blockProto );
        }

        private void RemoveBlockFromList(BlockProto blockProto)
        {
            _listFacade.RemoveItem( (dc) => dc.Data == blockProto );
        }

        private void HandleListEvent(MenuEventArgs args)
        {
            switch (args.id) {
                case addCmdId:
                    _blockProtoAPI.AddEmpty();
                    break;

                case removeCmdId:
                    foreach (var dc in args.dataContainers) {
                        if (dc.Data is BlockProto blockProto) {
                            blockProto.Destroy();
                        } else {
                            throw new Exception( "WTF this is not BlockProto!" );
                        }
                    }
                    break;

                case editCmdId:
                    if (args.dataContainers != null && args.dataContainers.Length == 1) {
                        if (args.dataContainers[0].Data is BlockProto blockProto) {
                            onStartEdit?.Invoke( blockProto.Key );
                        } else {
                            throw new Exception( $"WTF this is not {nameof( BlockProto )}!" );
                        }
                    }
                    break;

                default:
                    onListAction?.Invoke( args.id, args.dataContainers );
                    break;
            }
        }

        public void AddControllButton(string id, string text)
        {
            _listFacade.AddButton( id, text );
        }

        public void Show(bool show)
        {
            _listFacade.gameObject.SetActive( show );
        }
    }
}