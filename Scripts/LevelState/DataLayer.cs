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

    public class ChunkLayerChangedEventArgs<TData> : DataLayerEventArgs
    {
        public IEnumerable<Info> changed;

        public ChunkLayerChangedEventArgs(
            DataLayer dataLayer,
            IEnumerable<Info> changed = null) : base(dataLayer)
        {
            this.changed = changed;
        }

        public struct Info
        {
            public ChunkDataKey key;
            public TData data;
        }
    }

    public class BlockLayerChangedEventArgs : DataLayerEventArgs
    {
        public IEnumerable<Info> changed;

        public BlockLayerChangedEventArgs(
            DataLayer dataLayer,
            IEnumerable<Info> changed = null) : base(dataLayer)
        {
            this.changed = changed;
        }

        public struct Info
        {
            //public Vector3Int globalCoord;
            public ChunkDataKey dataKey;

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

        private bool _inTransaction;
        private HashSet<ChunkDataKey> _transactionLog;

        public SimpleChunkLayer(DataLayerSettings settings, ChunkStorage chunkStorage) : base(settings)
        {
            _chunkStorage = chunkStorage;
        }

        public void StartTransaction()
        {
            if (_inTransaction) {
                throw new Exception();
            }
            _inTransaction = true;
            _transactionLog = new();
        }

        public void StopTransaction()
        {
            if (_transactionLog != null) {
                // TODO remove this strong shit
                var args = new ChunkLayerChangedEventArgs<TData>(
                    this,
                    _transactionLog.Select(x =>
                        new ChunkLayerChangedEventArgs<TData>.Info() {
                            key = x,
                            data = GetData(x)
                        }
                    )
                );
                changed?.Invoke(args);
                _transactionLog = null;
            }
            _inTransaction = false;
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
            if (_inTransaction) {
                _transactionLog.Add(key);
            } else {
                // TODO Слишком мощно для одного параметра
                changed?.Invoke(
                    new ChunkLayerChangedEventArgs<TData>(
                        this,
                        new ChunkLayerChangedEventArgs<TData>.Info[] {
                            new ChunkLayerChangedEventArgs<TData>.Info() {
                                key = key,
                                data = data
                            }
                        }
                    )
                );
            }
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