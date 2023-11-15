using System;
using System.Numerics;
using RuntimeEditTools;

namespace Level
{
    /// <summary>
    /// Абстрактный слой данных.
    /// <para>
    /// Слой данных может хранить как структурные, так и объектные данные.
    /// Слой может быть как разбит на чанки, так и не разбит.
    /// </para>
    /// <para>
    /// Если слой разбит на чанки, глобальная индексация данных в таком слое не имеет смысла. 
    /// В таком случае навигация по данным осуществляется или через непосредственное 
    /// указание чанка и индекса единицы данных в нём, или через специальный навигационный тип.
    /// Допустим, к блоку в чанке можно обратиться через связку Vector3Int + uint 
    /// или через глобальный Vector3Int блока.
    /// </para>
    /// <para>
    /// Если слой на чанки не разбит, мы обзываем его индексным, потому что 
    /// глобальная индексация для него имеет смысл.
    /// Такие слои нужны для всяких дальнодействующих операций, движущихся между 
    /// чанками объектов и хранения архивных данных, типа инвентарей отсутствующих игроков.
    /// </para>
    /// <para>
    /// Слои одного или даже разных гридов могут быть логически связаны между собой. 
    /// Но это уже проблема обработчков слоёв.
    /// </para>
    /// </summary>
    public abstract class DataLayer<T>
    {
        public Action<int> onChanged;
        public abstract LayerType LayerType { get; }
        public string Tag => _tag;


        private string _tag;
        protected T[] _data;

        public DataLayer(string tag)
        {
            _tag = tag;
        }

        public T this[uint id]{
            get{
                return _data[id];
            }
            set{
                _data[id] = value;
            }
        }
    }

    public struct ChunkDataKey{
        public Vector3Int chunkCoord;
        public ushort dataId;
    }

    public abstract class IndexLayer<TData>:DataLayer<TData>
    {
        public IndexLayer(string tag):base(tag){}
    }

    /// <summary>
    /// Долговременное хранилище чанков.
    /// </summary>
    public abstract class ChunkStorage{
        public abstract object LoadChunk(Vector3Int coord);
        public abstract object SaveChunk(Vector3Int coord, object currentData);
        public abstract object RemoveAllChunks();
        public abstract Vector3Int[] GetExistedChunks();
    }


    /// <summary>
    /// Чанковый слой разбивается на большие пространственные блоки и хранит 
    /// данные порциями завязанными на положении точек привязки данных в пространстве.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TGlobalDataKey"></typeparam>
    public abstract class ChunkLayer<TData, TGlobalDataKey>:DataLayer<TData>{

        ChunkStorage _chunkStorage;
        Dictionary<Vector3Int, DataLayerContent<TData>> _loadedChunks;

        public ChunkLayer(string tag, ChunkStorage chunkStorage):base(tag){
            _chunkStorage = chunkStorage;
        }

        public abstract TData GetData(TGlobalDataKey key);

        public TData GetData(ChunkDataKey key){
            var chunkData = GetChunkData(key.chunkCoord);
            return chunkData[key.dataId];
        }
        public void PreloadChunks(Vector3Int[] chunkCoords){
            foreach(var coord in chunkCoords){
                _ = GetChunkData(coord);
            }
        }

        private DataLayerContent<TData> GetChunkData(Vector3Int coord){
            DataLayerContent<TData> data;
            if(!_loadedChunks.TryGetValue(coord, out data)){
                data = (DataLayerContent<TData>)_chunkStorage.LoadChunk(coord);
                _loadedChunks.Add(coord, data);
            }
            return data;
        }
    }


    public abstract class DataLayerContent<T> {

        protected abstract T GetData(uint id);
        protected abstract void SetData(uint id, T value);

        public T this[uint id]{
            get => GetData(id);
            set => SetData(id, value);
        }
    }

    public class DataLayerStaticContent<T>:DataLayerContent<T>{
        private T[] _data;
        public DataLayerStaticContent(uint size){
            _data = new T[size];
        }

        protected override T GetData(uint id)=>_data[id];

        protected override void SetData(uint id, T value)=>_data[id] = value;
    }

    public class DataLayerDynamicContent<T>:DataLayerContent<T>
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

        public void RemoveData(uint id){
            _data.Remove(id);
        }
    }

    // public abstract class DataLayer<T> : DataLayer
    // {
    //     private T[] _data;

    //     protected DataLayer(int size, string tag) : base( tag )
    //     {
    //         _data = new T[size];
    //     }

    //     protected DataLayer(int size, string tag, T[] data) : base( tag )
    //     {
    //         if (data == null) {
    //             throw new ArgumentNullException();
    //         }
    //         if (size != data.Length) {
    //             throw new ArgumentException();
    //         }
    //         _data = data;
    //     }

    //     public T Item(int index) => _data[index];

    //     public void SetItem(int index, T value)
    //     {
    //         _data[index] = value;
    //         onChanged?.Invoke( index );
    //     }

    //     internal T[] Data => _data;
    // }

    // public class DataLayerFabric
    // {
    //     public DataLayer Create(LayerType layerType, string tag, int size)
    //     {
    //         switch (layerType) {
    //             case LayerType.BlockLayer:
    //                 return new BlockLayer( size, tag );

    //             default:
    //                 throw new ArgumentException();
    //         }
    //     }

    //     public DataLayer Create<T>(LayerType layerType, string tag, int size, T[] data)
    //     {
    //         switch (layerType) {
    //             case LayerType.BlockLayer:
    //                 return new BlockLayer( size, tag, data as BlockData[] );

    //             default:
    //                 throw new ArgumentException();
    //         }
    //     }
    // }
}