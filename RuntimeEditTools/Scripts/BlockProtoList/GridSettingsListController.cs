using Level;
using Level.API;
using LevelView;
using System;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    internal class GridSettingsListController
    {
        private const string addCmdId = "ADD";
        private const string removeCmdId = "REMOVE";
        private const string editCmdId = "EDIT";

        private InteractiveListFacade _listFacade;
        private GridSettingsCollection _gridSettingsAPI;

        public UnityAction<uint> onStartEdit;
        public UnityAction<string, IDataContainer[]> onListAction;

        public GridSettingsListController(InteractiveListFacade listFacade, GridSettingsCollection gridSettingsAPI)
        {
            _listFacade = listFacade;
            _gridSettingsAPI = gridSettingsAPI;

            // From model to view
            foreach (var gridSettings in gridSettingsAPI) {
                AddGridSettingsToList( gridSettings );
            }

            // From view to model
            _listFacade.SendEvent += HandleListEvent;
        }

        internal void Show(bool show)
        {
            _listFacade.gameObject.SetActive( show );
        }

        public void Destroy()
        {
            _listFacade.SendEvent -= HandleListEvent;
        }

        public void AddControllButton(string id, string text)
        {
            _listFacade.AddButton( id, text );
        }

        private void HandleListEvent(MenuEventArgs args)
        {
            switch (args.id) {
                case addCmdId:
                    _gridSettingsAPI.Add();
                    break;

                case removeCmdId:
                    foreach (var dc in args.dataContainers) {
                        if (dc.Data is GridSettings gridSettings) {
                            gridSettings.Destroy();
                            _listFacade.RemoveItem( x => x.Data == gridSettings );
                        } else {
                            throw new Exception( "WTF this is not BlockProto!" );
                        }
                    }
                    break;

                case editCmdId:
                    if (args.dataContainers != null && args.dataContainers.Length == 1) {
                        if (args.dataContainers[0].Data is GridSettings gridSettings) {
                            onStartEdit?.Invoke( gridSettings.Key );
                        } else {
                            throw new Exception( $"WTF this is not {nameof( GridSettings )}!" );
                        }
                    }
                    break;

                default:
                    onListAction?.Invoke( args.id, args.dataContainers );
                    break;
            }
        }

        private void AddGridSettingsToList(GridSettings gridSettings)
        {
            _listFacade.AddItem( gridSettings.Name, gridSettings );
        }

        private void RemoveGridSettingsFromList(GridSettings gridSettings)
        {
            _listFacade.RemoveItem( x => x.Data == gridSettings );
        }
    }
}