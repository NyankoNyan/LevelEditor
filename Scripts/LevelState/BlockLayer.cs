using System;

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
    }

    public class BlockLayer : DataLayer<BlockData>
    {
        public BlockLayer(int size, string tag) : base( size, tag ) { }
        public BlockLayer(int size, string tag, BlockData[] data) : base( size, tag, data ) { }

        public override LayerType LayerType => LayerType.BlockLayer;
    }
}
