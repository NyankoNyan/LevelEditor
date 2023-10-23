using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Level.API
{
    public interface IGridStatesAPI
    {
        UnityAction<GridState> onStateAdded { get; set; }
        GridState AddState(string gridSettingsName);
        GridState AddState(string gridSettingsName, uint id);
        void RemoveState(uint id);
        IEnumerable<GridState> Grids { get; }
    }

    internal class GridStatesAPI : IGridStatesAPI
    {
        private GridStateRegistry _gridStateRegistry;
        private GridStateFabric _gridStateFabric;
        private IGridSettingsAPI _gridSettingsAPI;
        private DataLayerFabric _dataLayerFabric;

        //private DataLayerAPIFabric _dataLayerAPIFabric;

        public GridStatesAPI(GridStateRegistry gridStateRegistry, GridStateFabric gridStateFabric, IGridSettingsAPI gridSettingsAPI, DataLayerFabric dataLayerFabric)
        {
            _gridStateRegistry = gridStateRegistry;
            _gridStateFabric = gridStateFabric;
            _gridSettingsAPI = gridSettingsAPI;
            //_dataLayerAPIFabric = dataLayerAPIFabric;
            _dataLayerFabric = dataLayerFabric;
        }

        public IEnumerable<GridState> Grids => _gridStateRegistry.Values;

        public UnityAction<GridState> onStateAdded { get => _gridStateFabric.onCreate; set => _gridStateFabric.onCreate = value; }

        public GridState AddState(string gridSettingsName)
        {
            var gridSettings = _gridSettingsAPI.GetGridSettings( gridSettingsName );
            var createParams = new GridStateCreateParams( gridSettings, _dataLayerFabric );
            return _gridStateFabric.Create( createParams );
        }

        public GridState AddState(string gridSettingsName, uint id)
        {
            var gridSettings = _gridSettingsAPI.GetGridSettings( gridSettingsName );
            var createParams = new GridStateCreateParams( gridSettings, _dataLayerFabric );
            return _gridStateFabric.CreateWithCounter( createParams, id );
        }

        public void RemoveState(uint id)
        {
            GridState gridState = null;
            try {
                gridState = _gridStateRegistry.Values.First( x => x.Key == id );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing grid instance with id {id}" );
            }
            gridState.Destroy();
        }
    }
}
