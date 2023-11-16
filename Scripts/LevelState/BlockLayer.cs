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

    public class BlockLayer<TData> : ChunkLayer<TData, Vector3Int>
    {
        private Vector3Int _size;

        public BlockLayer(string tag, Vector3Int size, ChunkStorage chunkStorage) : base( tag, chunkStorage )
        {
            if (size.x <= 0 || size.y <= 0 || size.z <= 0) {
                throw new Exception( $"Bad chunk size {size} for layer with tag {tag}" );
            }
            _size = size;
        }

        public override LayerType LayerType => LayerType.BlockLayer;

        public override TData GetData(Vector3Int key)
        {
            Vector3Int chunkCoord = new Vector3Int(
                key.x / _size.x - ( key.x < 0 ? 1 : 0 ),
                key.y / _size.y - ( key.y < 0 ? 1 : 0 ),
                key.z / _size.z - ( key.z < 0 ? 1 : 0 )
                );
            Vector3Int blockCoord = key - Vector3Int.Scale( chunkCoord, _size );
            ushort id = (ushort)GridState.BlockCoordToFlat( blockCoord, _size );

            return GetData( new ChunkDataKey( chunkCoord, id ) );
        }

        public override void SetData(Vector3Int key, TData data)
        {
            Vector3Int chunkCoord = new Vector3Int(
                key.x / _size.x - ( key.x < 0 ? 1 : 0 ),
                key.y / _size.y - ( key.y < 0 ? 1 : 0 ),
                key.z / _size.z - ( key.z < 0 ? 1 : 0 )
                );
            Vector3Int blockCoord = key - Vector3Int.Scale( chunkCoord, _size );
            ushort id = (ushort)GridState.BlockCoordToFlat( blockCoord, _size );

            SetData( new ChunkDataKey( chunkCoord, id ), data );
        }
    }
}