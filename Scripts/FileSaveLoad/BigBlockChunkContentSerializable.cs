using System;
using System.Collections.Generic;

namespace Level.IO
{
    [Serializable]
    public struct BigBlockChunkContentSerializable
    {
        public int[] ids;
        public BigBlockData[] data;

        public static explicit operator BigBlockChunkContentSerializable(DataLayerDynamicContent<BigBlockData> content)
        {
            List<int> ids = new();
            List<BigBlockData> data = new();
            foreach (var kvp in content.Items) {
                ids.Add(kvp.Key);
                data.Add(kvp.Value);
            }
            BigBlockChunkContentSerializable result = new() {
                ids = ids.ToArray(),
                data = data.ToArray()
            };
            return result;
        }

        public void Load(DataLayerDynamicContent<BigBlockData> content)
        {
            for (int i = 0; i < ids.Length; i++) {
                content[ids[i]] = data[i];
            }
        }
    }
}