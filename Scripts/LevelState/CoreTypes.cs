using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Level
{
    public struct ChunkBlockKey
    {
        public Vector3Int chunkCoord;
        public Vector3Int blockCoord;

        public ChunkBlockKey(Vector3Int chunkCoord, Vector3Int blockCoord)
        {
            this.chunkCoord = chunkCoord;
            this.blockCoord = blockCoord;
        }

        public static implicit operator ChunkBlockKey(Tuple<Vector3Int, Vector3Int> value)
            => new ChunkBlockKey( value.Item1, value.Item2 );
    }

    public struct ChunkDataKey
    {
        public Vector3Int chunkCoord;
        public ushort dataId;

        public ChunkDataKey(Vector3Int chunkCoord, ushort dataId)
        {
            this.chunkCoord = chunkCoord;
            this.dataId = dataId;
        }
    }
}
