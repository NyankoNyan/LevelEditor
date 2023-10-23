using System;
using System.Linq;
using UnityEngine;

namespace Level.IO
{
    [Serializable]
    internal class ChunkSerializable
    {
        public Vector3Int id;
        public DataLayerSerializable[] dataLayer;

        public static explicit operator ChunkSerializable(GridChunk gridChunk)
            => new() {
                id = gridChunk.Key,
                dataLayer = gridChunk.Layers.Select( x => (DataLayerSerializable)x ).ToArray(),
            };

        public void Load(GridState gridState)
        {
            var chunk = gridState.GetChunk( id );
            foreach (DataLayerSerializable layerSerial in dataLayer) {
                DataLayer layer;
                layer = chunk.GetLayer( layerSerial.layerType, layerSerial.tag );

                if (layer.LayerType == LayerType.BlockLayer) {
                    BlockLayer blockLayer = layer as BlockLayer;
                    for (int i = 0; i < blockLayer.Data.Length; i++) {
                        blockLayer.Data[i].blockId = layerSerial.blockDataIds[i];
                        blockLayer.Data[i].rotation = layerSerial.blockDataRotations[i];
                    }
                }
            }
        }
    }
}