using System;

using UnityEngine;

namespace Level
{
    /// <summary>
    /// Большие блоки - это специальная сущность обозначающая блоки,
    /// способные занимать пространства больше одного блока и выходить за пределы чанка.
    /// С точки зрения логики они должны обрабатываться как одна сующность.
    /// При использовании совместно с BlockData, все операции с блоками занимаемыми большим блоком
    /// должны осуществляться через большие блоки.
    /// </summary>
    [Serializable]
    public struct BigBlockData
    {
        /// <summary>
        /// Смещение до блока, в котором находится якорный блок большого блока.
        /// </summary>
        public Vector3Int blockOffset;

        /// <summary>
        /// По сути это тоже самое, что BlockProto. Настраивается там же.
        /// Обозвано иначе, чтобы можно было разделить сущности.
        /// </summary>
        public uint BigBlockProto;

        /// <summary>
        /// Идентификатор блока в якорном чанке
        /// </summary>
        public ushort id;

        /// <summary>
        /// Вращение.
        /// </summary>
        /// <remarks>
        /// Биты 0-2 кодируют знаковую ось xyz по которой выровнен блок.
        /// Биты 3-4 кодируют само вращение (0, 90, 180, 270).
        /// </remarks>
        public byte rotation;
    }

    /// <summary>
    /// Хранит данные для больших блоков
    /// </summary>
    public class BigBlockLayer<TData> : SimpleChunkLayer<TData>
    {
        public BigBlockLayer(
            DataLayerSettings settings,
            ChunkStorage chunkStorage)
            : base(settings, chunkStorage)
        {
        }

        public override LayerType LayerType => LayerType.BigBlockLayer;
    }
}