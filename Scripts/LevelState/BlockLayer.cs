using Level.API;

using System;

using UnityEngine;

namespace Level
{
    //TODO Можно сократить blockId до byte и получить выравнивание получше или добавить ещё байт инфы
    //TODO Можно ужать blockId до 11 бит и получить 2048 вариантов блоков
    public struct BlockData
    {
        public ushort blockId;

        /// <summary>
        /// Вращение.
        /// </summary>
        /// <remarks>
        /// Биты 0-2 кодируют знаковую ось xyz по которой выровнен блок.
        /// Биты 3-4 кодируют само вращение (0, 90, 180, 270).
        /// </remarks>
        public byte rotation;

        public BlockData(ushort blockId, byte rotation)
        {
            this.blockId = blockId;
            this.rotation = rotation;
        }
    }

    public struct ClientViewData
    {
        public GameObject gameObject;

        public ClientViewData(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }

    public class BlockLayer<TData> : ChunkLayer<TData, Vector3Int>
    {
        private readonly Vector3Int _size;

        public BlockLayer(DataLayerSettings settings, ChunkStorage chunkStorage) : base( settings, chunkStorage )
        {
            _size = settings.chunkSize;
            if (_size.x <= 0 || _size.y <= 0 || _size.z <= 0) {
                throw new Exception( $"Bad chunk size {_size} for layer with tag {settings.tag}" );
            }
        }

        public Vector3Int ChunkSize => _size;

        public override LayerType LayerType => LayerType.BlockLayer;

        public int ChunkFlatSize => _size.x * _size.y * _size.z;

        public Vector3Int GetChunkOfGlobalBlock(Vector3Int blockCoord)
        {
            Vector3Int result = default;
            for(int i = 0; i < 3; i++) {
                int signOne = blockCoord[i] < 0 ? 1 : 0;
                result[i] = (blockCoord[i] + signOne) / _size[i] - signOne;
            }
            return result;
        }

        public Vector3Int LocalCoordOfGlobalBlock(Vector3Int blockCoord)
        {
            Vector3Int chunkCoord = GetChunkOfGlobalBlock( blockCoord );
            return blockCoord - Vector3Int.Scale( chunkCoord, _size );
        }

        public int LocalIdOfGlobalBlock(Vector3Int blockCoord)
        {
            Vector3Int localCoord = LocalCoordOfGlobalBlock( blockCoord );
            return GridState.BlockCoordToFlat( localCoord, _size );
        }

        public Vector3Int BlockGlobalCoord(Vector3Int chunkCoord, int blockId)
        {
            return Vector3Int.Scale( chunkCoord, _size ) + GridState.FlatToBlockCoordSafe( blockId, _size );
        }

        public override TData GetData(Vector3Int key)
        {
            Vector3Int chunkCoord = GetChunkOfGlobalBlock( key );
            ushort id = (ushort)LocalIdOfGlobalBlock( key );
            return GetData( new ChunkDataKey( chunkCoord, id ) );
        }

        public override void SetData(Vector3Int key, TData data)
        {
            Vector3Int chunkCoord = GetChunkOfGlobalBlock(key);
            Vector3Int blockCoord = key - Vector3Int.Scale( chunkCoord, _size );
            ushort id = (ushort)GridState.BlockCoordToFlat( blockCoord, _size );

            SetData( new ChunkDataKey( chunkCoord, id ), data );
        }
    }
}