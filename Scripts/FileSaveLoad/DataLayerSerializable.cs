using System;

namespace Level.IO
{
    [Serializable]
    internal class DataLayerSerializable
    {
        public LayerType layerType;
        public string tag;
        public ushort[] blockDataIds;
        public byte[] blockDataRotations;

        public static explicit operator DataLayerSerializable(DataLayer dataLayer)
        {
            DataLayerSerializable result = new();
            result.layerType = dataLayer.LayerType;
            result.tag = dataLayer.Tag;

            if (dataLayer is BlockLayer) {
                BlockData[] blockData = ( dataLayer as BlockLayer ).Data;
                result.blockDataIds = new ushort[blockData.Length];
                result.blockDataRotations = new byte[blockData.Length];
                for (int i = 0; i < blockData.Length; i++) {
                    result.blockDataIds[i] = blockData[i].blockId;
                    result.blockDataRotations[i] = blockData[i].rotation;
                }
            } else {
                throw new ArgumentException( $"Unknown type {dataLayer.GetType()}" );
            }

            return result;
        }

        public DataLayer Load(DataLayerFabric dataLayerFabric)
        {
            if (layerType == LayerType.BlockLayer) {
                BlockData[] blockData = new BlockData[blockDataIds.Length];
                for (int i = 0; i < blockData.Length; i++) {
                    blockData[i] = new BlockData() {
                        blockId = blockDataIds[i],
                        rotation = blockDataRotations[i]
                    };
                }
                return dataLayerFabric.Create( layerType, tag, blockData.Length, blockData );
            } else {
                throw new Exception();
            }

        }
    }
}
