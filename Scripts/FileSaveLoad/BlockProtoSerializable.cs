using Level.API;
using System;

namespace Level.IO
{
    [Serializable]
    internal class BlockProtoSerializable
    {
        public uint id;
        public BlockProtoSettings settings;

        public static explicit operator BlockProtoSerializable(BlockProto blockProto)
            => new() {
                id = blockProto.Key,
                settings = blockProto.Settings
            };

        public void Load(IBlockProtoAPI blockProtoAPI)
        {
            blockProtoAPI.Add( settings );
        }
    }
}
