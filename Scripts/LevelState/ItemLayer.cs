using System.Collections;

using UnityEngine;

namespace Level
{
    public enum ItemLocationType
    {
        None, //Предмет не существует
        Free, //Предмет ни к чему непривязан
        Local, //Предмет подчинён другому предмету
        Slot, //Предмет находится в слоте
        BoneLink // Предмет привязан к костям
    }

    public struct BoneInfo
    {
        public int boneId;
        public float weight;
    }

    /// <summary>
    /// Ну типа эта структура, которая safe fixed array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Fixed4<T> : IEnumerable<T>
    {
        public T v0, v1, v2, v3;

        public T this[int index] {
            get {
                switch (index) {
                    case 0: return v0;
                    case 1: return v1;
                    case 2: return v2;
                    case 3: return v3;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: v0 = value; break;
                    case 1: v1 = value; break;
                    case 2: v2 = value; break;
                    case 3: v3 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return v0;
            yield return v1;
            yield return v2;
            yield return v3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public struct ItemData
    {
        public uint id;
        public uint lockUser;
        public ItemLocationType locationType;
        public uint parentItemId;
        public uint parentItemSlot;
        public Vector3 localPositions;
        public Quaternion localRotaion;
        public Fixed4<BoneInfo> bones;
    }

    public class GlobalItemLayer : IndexLayer<ItemData>
    {

        public GlobalItemLayer(DataLayerSettings settings) : base(settings) { }

        public override LayerType LayerType => LayerType.GlobalItemLayer;
    }

    public class AttachedItemLayer : SimpleChunkLayer<ItemData>
    {
        public AttachedItemLayer(DataLayerSettings settings, ChunkStorage chunkStorage) : base(settings, chunkStorage) { }

        public override LayerType LayerType => LayerType.ItemLayer;
    }
}