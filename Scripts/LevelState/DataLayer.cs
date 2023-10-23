using System;
using UnityEngine.Events;

namespace Level
{
    public abstract class DataLayer
    {
        public UnityAction<int> onChanged;
        //private bool _changed;
        private string _tag;

        public abstract LayerType LayerType { get; }
        public string Tag => _tag;
        public DataLayer(string tag)
        {
            _tag = tag;
        }
        //public void RegChanges() => _changed = true;
        //public void FlushChanges()
        //{
        //    if (_changed) {
        //        if (onChanged != null) {
        //            onChanged();
        //        }
        //        _changed = false;
        //    }
        //}
    }

    public abstract class DataLayer<T> : DataLayer
    {
        private T[] _data;

        protected DataLayer(int size, string tag) : base( tag )
        {
            _data = new T[size];
        }

        protected DataLayer(int size, string tag, T[] data) : base( tag )
        {
            if (data == null) {
                throw new ArgumentNullException();
            }
            if (size != data.Length) {
                throw new ArgumentException();
            }
            _data = data;
        }

        public T Item(int index) => _data[index];
        public void SetItem(int index, T value)
        {
            _data[index] = value;
            onChanged?.Invoke( index );
        }
        internal T[] Data => _data;
    }


    public class DataLayerFabric
    {
        public DataLayer Create(LayerType layerType, string tag, int size)
        {
            switch (layerType) {
                case LayerType.BlockLayer:
                    return new BlockLayer( size, tag );
                default:
                    throw new ArgumentException();
            }
        }

        public DataLayer Create<T>(LayerType layerType, string tag, int size, T[] data)
        {
            switch (layerType) {
                case LayerType.BlockLayer:
                    return new BlockLayer( size, tag, data as BlockData[] );
                default:
                    throw new ArgumentException();
            }
        }
    }
}
