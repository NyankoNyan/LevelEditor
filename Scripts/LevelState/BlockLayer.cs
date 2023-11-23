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

        public static Quaternion DecodeRotation(byte rotation)
        {
            Quaternion firstRot;
            Quaternion secondRot;

            switch ((Angle)(rotation & 24)) {
                case Angle.Deg90:
                    firstRot = Quaternion.AngleAxis(90, Vector3.up);
                    break;

                case Angle.Deg180:
                    firstRot = Quaternion.AngleAxis(180, Vector3.up);
                    break;

                case Angle.Deg270:
                    firstRot = Quaternion.AngleAxis(270, Vector3.up);
                    break;

                default:
                    firstRot = Quaternion.identity;
                    break;
            }

            switch ((Axis)(rotation & 7)) {
                case Axis.PlusX:
                    secondRot = Quaternion.AngleAxis(-90, Vector3.forward);
                    break;

                case Axis.MinusX:
                    secondRot = Quaternion.AngleAxis(90, Vector3.forward);
                    break;

                case Axis.PlusY:
                    secondRot = Quaternion.identity;
                    break;

                case Axis.MinusY:
                    secondRot = Quaternion.AngleAxis(180, Vector3.forward);
                    break;

                case Axis.PlusZ:
                    secondRot = Quaternion.AngleAxis(90, Vector3.right);
                    break;

                case Axis.MinusZ:
                    secondRot = Quaternion.AngleAxis(-90, Vector3.right);
                    break;

                default:
                    throw new LevelAPIException($"Unknown rotation {rotation}");
            }

            return secondRot * firstRot;
        }

        public enum Axis
        {
            PlusX = 0,
            PlusY = 1,
            PlusZ = 2,
            MinusX = 4,
            MinusY = 5,
            MinusZ = 6
        }

        public enum Angle
        {
            Deg0 = 0,
            Deg90 = 8,
            Deg180 = 16,
            Deg270 = 24
        }
    }

    public class BlockLayer<TData> : ChunkLayer<TData, Vector3Int>
    {
        private readonly Vector3Int _size;

        public BlockLayer(string tag, Vector3Int size, ChunkStorage chunkStorage) : base(tag, chunkStorage)
        {
            if (size.x <= 0 || size.y <= 0 || size.z <= 0) {
                throw new Exception($"Bad chunk size {size} for layer with tag {tag}");
            }
            _size = size;
        }

        public Vector3Int ChunkSize => _size;

        public override LayerType LayerType => LayerType.BlockLayer;

        public int ChunkFlatSize => _size.x * _size.y * _size.z;

        public override TData GetData(Vector3Int key)
        {
            Vector3Int chunkCoord = new Vector3Int(
                key.x / _size.x - (key.x < 0 ? 1 : 0),
                key.y / _size.y - (key.y < 0 ? 1 : 0),
                key.z / _size.z - (key.z < 0 ? 1 : 0)
                );
            Vector3Int blockCoord = key - Vector3Int.Scale(chunkCoord, _size);
            ushort id = (ushort)GridState.BlockCoordToFlat(blockCoord, _size);

            return GetData(new ChunkDataKey(chunkCoord, id));
        }

        public override void SetData(Vector3Int key, TData data)
        {
            Vector3Int chunkCoord = new Vector3Int(
                key.x / _size.x - (key.x < 0 ? 1 : 0),
                key.y / _size.y - (key.y < 0 ? 1 : 0),
                key.z / _size.z - (key.z < 0 ? 1 : 0)
                );
            Vector3Int blockCoord = key - Vector3Int.Scale(chunkCoord, _size);
            ushort id = (ushort)GridState.BlockCoordToFlat(blockCoord, _size);

            SetData(new ChunkDataKey(chunkCoord, id), data);
        }
    }
}