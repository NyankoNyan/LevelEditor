using System.Collections;

namespace Level{
    /// <summary>
    /// Содержимое слоя данных
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataLayerContent<T>:IEnumerable<T>
    {
        protected abstract T GetData(uint id);

        protected abstract void SetData(uint id, T value);

        public abstract IEnumerator<T> GetEnumerator();

        private IEnumerator<T> GetEnumerator1() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator1();
        
        public T this[uint id]
        {
            get => GetData( id );
            set => SetData( id, value );
        }

    }

    /// <summary>
    /// Хранилище элементов слоя данных с неизменяемым числом элементов.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataLayerStaticContent<T> : DataLayerContent<T>
    {
        private T[] _data;

        public DataLayerStaticContent(uint size)
        {
            _data = new T[size];
        }

        public override IEnumerator<T> GetEnumerator()=>(IEnumerator<T> )_data.GetEnumerator();

        protected override T GetData(uint id) => _data[id];

        protected override void SetData(uint id, T value) => _data[id] = value;
    }

    public class DataLayerDynamicContent<T> : DataLayerContent<T>
    {
        private Dictionary<uint, T> _data = new();

        protected override T GetData(uint id)
        {
            return _data[id];
        }

        protected override void SetData(uint id, T value)
        {
            _data[id] = value;
        }

        public void RemoveData(uint id)
        {
            _data.Remove( id );
        }

        public override IEnumerator<T> GetEnumerator()=>_data.Values.GetEnumerator();

        public Dictionary<uint, T> Items => _data;
    }

}