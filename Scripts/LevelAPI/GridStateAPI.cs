using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level.API
{
    public interface IGridStateAPI
    {
        IEnumerable<IDataLayerAPI> Layers { get; }
        uint Id { get; }
        GridSettings GridSettings { get; }
        GridChunk GetChunk(Vector3Int coord);
    }

    internal class GridStateAPI : IGridStateAPI
    {
        private GridState _gridState;
        private DataLayerAPIFabric _dataLayerFabric;

        public GridStateAPI(GridState gridState, DataLayerAPIFabric dataLayerFabric)
        {
            _gridState = gridState;
            _dataLayerFabric = dataLayerFabric;
        }

        public IEnumerable<IDataLayerAPI> Layers => _gridState.GridSettings.Settings.layers.Select( x => _dataLayerFabric.Create( x, _gridState ) );

        public uint Id => _gridState.Key;

        public GridSettings GridSettings => _gridState.GridSettings;

        public GridChunk GetChunk(Vector3Int coord)
        {
            throw new System.NotImplementedException();
        }
    }

}
