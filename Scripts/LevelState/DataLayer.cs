using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level
{
    [Serializable]
    public struct DataLayerSettings
    {
        public LayerType layerType;
        public Vector3Int chunkSize;
        public string tag;
    }

    public abstract class DataLayerEventArgs
    {
        public DataLayer dataLayer;
    }

    public class BlockLayerEventArgs : DataLayerEventArgs
    {
        public Vector3Int blockCoord;
    }

    public class BlockLevelLoadedEventArgs : BlockLayerEventArgs
    { }

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

        public string Tag => _tag;

        private string _tag;

        public DataLayer(string tag)
        {
            _tag = tag;
        }
    }

    /// <summary>
    /// Слой с доступом по глобальному индексу.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public abstract class IndexLayer<TData> : DataLayer
    {
        public IndexLayer(string tag) : base( tag )
        {
        }
    }

    /// <summary>
    /// Чанковый слой разбивается на большие пространственные блоки и хранит
    /// данные порциями завязанными на положении точек привязки данных в пространстве.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TGlobalDataKey"></typeparam>
    public abstract class ChunkLayer<TData, TGlobalDataKey> : DataLayer
    {
        //TODO Chunk removing
        public Action<Vector3Int> chunkRemoved;
        public Action<Vector3Int> chunkAdded;

        private ChunkStorage _chunkStorage;
        private Dictionary<Vector3Int, DataLayerContent<TData>> _loadedChunks;

        public ChunkLayer(string tag, ChunkStorage chunkStorage) : base( tag )
        {
            _chunkStorage = chunkStorage;
        }

        public abstract TData GetData(TGlobalDataKey key);
        public abstract void SetData(TGlobalDataKey key, TData data);

        public TData GetData(ChunkDataKey key)
        {
            var chunkData = GetChunkData( key.chunkCoord );
            return chunkData[key.dataId];
        }

        public void SetData(ChunkDataKey key, TData data)
        {
            var chunkData = GetChunkData( key.chunkCoord );
            chunkData[key.dataId] = data;
        }

        public void PreloadChunks(Vector3Int[] chunkCoords)
        {
            foreach (var coord in chunkCoords) {
                _ = GetChunkData( coord );
            }
        }

        public Vector3Int[] LoadedChunks => _loadedChunks.Keys.ToArray();

        public DataLayerContent<TData> GetChunkData(Vector3Int coord)
        {
            DataLayerContent<TData> data;
            if (!_loadedChunks.TryGetValue( coord, out data )) {
                data = (DataLayerContent<TData>)_chunkStorage.LoadChunk( coord );
                _loadedChunks.Add( coord, data );
                chunkAdded?.Invoke( coord );
            }
            return data;
        }
    }
}