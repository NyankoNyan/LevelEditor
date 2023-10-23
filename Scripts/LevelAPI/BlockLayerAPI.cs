using System.Linq;

namespace Level.API
{
    public interface IBlockLayerAPI
    {
        BlockData GetBlockData(ChunkBlockKey chunkBlockKey);
    }

    internal class BlockLayerAPI : IDataLayerAPI, IBlockLayerAPI
    {
        private GridState _gridState;
        private string _layerTag;

        public LayerType LayerType => LayerType.BlockLayer;

        public BlockLayerAPI(GridState gridState, string layerTag)
        {
            _gridState = gridState;
            _layerTag = layerTag;
        }

        public BlockData GetBlockData(ChunkBlockKey chunkBlockKey)
        {
            var chunk = _gridState.GetChunk( chunkBlockKey.chunkCoord );
            DataLayer layer = chunk.Layers.First( x => x.Tag == _layerTag );
            int index = GridChunk.BlockCoordToFlat( chunkBlockKey.blockCoord, _gridState.GridSettings.ChunkSize );
            return ( layer as BlockLayer ).Item( index );

        }
    }
}
