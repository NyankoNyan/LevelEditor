using System;
using System.Linq;

namespace Level.IO
{
    [Serializable]
    public class BlockChunkConvertSerializable
    {
        public ushort[] blockDataIds;
        public byte[] blockDataRotations;

        public static explicit operator BlockChunkConvertSerializable(DataLayerStaticContent<BlockData> content)
        {
            BlockChunkConvertSerializable result = new();
            BlockData[] data = content.ToArray();
            result.blockDataIds = new ushort[data.Length];
            result.blockDataRotations = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                result.blockDataIds[i] = data[i].blockId;
                result.blockDataRotations[i] = data[i].rotation;
            }
            return result;
        }

        public void Load(DataLayerStaticContent<BlockData> content)
        {
            for (int i = 0; i < blockDataIds.Length; i++) {
                content[i] = new BlockData(blockDataIds[i], blockDataRotations[i]);
            }
        }
    }
}