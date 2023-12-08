using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace Level
{
    [Serializable]
    public struct DataLayerSettings : IEquatable<DataLayerSettings>
    {
        public LayerType layerType;
        public Vector3Int chunkSize;
        public string tag;
        public bool hasViewLayer;

        public DataLayerSettings(DataLayerSettings original)
        {
            layerType = original.layerType;
            chunkSize = original.chunkSize;
            tag = original.tag;
            hasViewLayer = original.hasViewLayer;
        }

        public bool Equals(DataLayerSettings other)
        {
            return layerType == other.layerType
                && tag == other.tag
                && chunkSize == other.chunkSize
                && hasViewLayer == other.hasViewLayer;
        }

    }

    public abstract class DataLayerEventArgs
    {
        public DataLayer dataLayer;

        public DataLayerEventArgs(DataLayer dataLayer)
        {
            Assert.IsNotNull(dataLayer);
            this.dataLayer = dataLayer;
        }
    }

    public class BlockLayerChangedEventArgs : DataLayerEventArgs
    {
        public IEnumerable<Info> added;
        public IEnumerable<Vector3Int> removed;

        public BlockLayerChangedEventArgs(
            DataLayer dataLayer,
            IEnumerable<Info> added = null,
            IEnumerable<Vector3Int> removed = null) : base(dataLayer)
        {
            this.added = added;
            this.removed = removed;
        }

        public struct Info
        {
            public Vector3Int globalCoord;
            public BlockData blockData;
        }
    }

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
    public abstract class DataLayer
    {
        public Action<DataLayerEventArgs> changed;

        public abstract LayerType LayerType { get; }

        public string Tag => _settings.tag;
        public DataLayerSettings Settings => _settings;

        private DataLayerSettings _settings;

        public DataLayer(DataLayerSettings settings)
        {
            _settings = settings;
        }
    }

    /// <summary>
    /// Слой с доступом по глобальному индексу.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public abstract class IndexLayer<TData> : DataLayer
    {
        public Action<TData> added;
        public Action<TData> removed;
        protected Dictionary<uint, TData> _data = new();
        public IndexLayer(DataLayerSettings settings) : base(settings)
        {

        }
        public void Add(uint id, TData value)
        {
            if (!_data.TryAdd(id, value)) {
                throw new Exception();
            }
            added?.Invoke(value);
        }

        public void Remove(uint id)
        {
            if (_data.TryGetValue(id, out TData value)) {
                _data.Remove(id);
                removed?.Invoke(value);
            } else {
                throw new Exception();
            }
        }
    }

    public abstract class SimpleChunkLayer<TData> : DataLayer
    {
        //TODO Chunk removing
        public Action<Vector3Int> chunkRemoved;

        public Action<Vector3Int> chunkAdded;

        private ChunkStorage _chunkStorage;
        private Dictionary<Vector3Int, DataLayerContent<TData>> _loadedChunks = new();

        public SimpleChunkLayer(DataLayerSettings settings, ChunkStorage chunkStorage) : base(settings)
        {
            _chunkStorage = chunkStorage;
        }

        public TData GetData(ChunkDataKey key)
        {
            var chunkData = GetChunkData(key.chunkCoord);
            return chunkData[key.dataId];
        }

        public void SetData(ChunkDataKey key, TData data)
        {
            var chunkData = GetChunkData(key.chunkCoord);
            chunkData[key.dataId] = data;
        }

        public void PreloadChunks(Vector3Int[] chunkCoords)
        {
            foreach (var coord in chunkCoords) {
                _ = GetChunkData(coord);
            }
        }

        public Vector3Int[] LoadedChunks => _loadedChunks.Keys.ToArray();

        public DataLayerContent<TData> GetChunkData(Vector3Int coord)
        {
            DataLayerContent<TData> data;
            if (!_loadedChunks.TryGetValue(coord, out data)) {
                data = (DataLayerContent<TData>)_chunkStorage.LoadChunk(coord);
                _loadedChunks.Add(coord, data);
                chunkAdded?.Invoke(coord);
            }
            return data;
        }
    }

    /// <summary>
    /// Чанковый слой разбивается на большие пространственные блоки и хранит
    /// данные порциями завязанными на положении точек привязки данных в пространстве.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TGlobalDataKey"></typeparam>
    public abstract class ChunkLayer<TData, TGlobalDataKey> : SimpleChunkLayer<TData>
    {
        public ChunkLayer(DataLayerSettings settings, ChunkStorage chunkStorage) : base(settings, chunkStorage)
        {
        }

        public abstract TData GetData(TGlobalDataKey key);
        public abstract void SetData(TGlobalDataKey key, TData data);
    }
}