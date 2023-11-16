using System;
using UnityEngine;

namespace Level
{
    /// <summary>
    /// Долговременное хранилище чанков.
    /// </summary>
    public abstract class ChunkStorage
    {
        public abstract object LoadChunk(Vector3Int coord);

        public abstract void SaveChunk(Vector3Int coord, object currentData);

        public abstract void RemoveAllChunks();

        public abstract Vector3Int[] GetExistedChunks();
    }

    public abstract class ChunkStorageFabric
    {
        public abstract ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings);
    }

    public class MockChunkStorageFabric : ChunkStorageFabric
    {
        private MockBlockChunkStorage _blockStorage;

        public override ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings)
        {
            switch (dataLayerSettings.layerType) {
                case LayerType.BlockLayer:
                    if (_blockStorage == null) {
                        _blockStorage = new MockBlockChunkStorage( dataLayerSettings.chunkSize );
                    }
                    return _blockStorage;

                default:
                    throw new Exception();
            }
        }
    }

    public class MockBlockChunkStorage : ChunkStorage
    {
        private Vector3Int _size;

        public MockBlockChunkStorage(Vector3Int size)
        {
            _size = size;
        }

        public override Vector3Int[] GetExistedChunks()
        {
            return new Vector3Int[0];
        }

        public override object LoadChunk(Vector3Int coord)
        {
            int flatSize = _size.x * _size.y * _size.z;
            DataLayerStaticContent<BlockData> content = new( (uint)flatSize );
            return content;
        }

        public override void RemoveAllChunks()
        {
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
        }
    }
}