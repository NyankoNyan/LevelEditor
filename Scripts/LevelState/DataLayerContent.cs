using System.Collections;
using System.Collections.Generic;

namespace Level
{
    /// <summary>
    /// Содержимое слоя данных
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataLayerContent<T> : IEnumerable<T>
    {
        protected abstract T GetData(int id);

        protected abstract void SetData(int id, T value);

        public abstract IEnumerator<T> GetEnumerator();

        private IEnumerator<T> GetEnumerator1() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();

        public T this[int id] {
            get => GetData(id);
            set => SetData(id, value);
        }
    }

    /// <summary>
    /// Хранилище элементов слоя данных с неизменяемым числом элементов.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataLayerStaticContent<T> : DataLayerContent<T>
    {
        private T[] _data;

        public int Size => _data.Length;

        public DataLayerStaticContent(int size)
        {
            _data = new T[size];
        }

        public override IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_data.GetEnumerator();

        protected override T GetData(int id) => _data[id];

        protected override void SetData(int id, T value) => _data[id] = value;
    }

    public class DataLayerDynamicContent<T> : DataLayerContent<T>
    {
        private Dictionary<int, T> _data = new();

        protected override T GetData(int id)
        {
            return _data[id];
        }

        protected override void SetData(int id, T value)
        {
            if (!_data.TryAdd(id, value)) {
                _data[id] = value;
            }
        }

        public void RemoveData(int id)
        {
            _data.Remove(id);
        }

        public override IEnumerator<T> GetEnumerator() => _data.Values.GetEnumerator();

        public Dictionary<int, T> Items => _data;
    }
}