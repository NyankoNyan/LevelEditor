using Level.API;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace Level
{
    public struct GridStateInfo
    {
        public uint id;
        public uint settingsId;
    }

    /// <summary>
    /// Хранит текущее состояние пространственной сетки и её содержимое.
    /// Содержимое хранится в виде независимых слоёв, каждый из которых представляет свой тип данных.
    /// Слой может быть разбит на чанки. Чанки разных слоёв могут быть загружены независимо друг от друга.
    /// </summary>
    public class GridState : IHasKey<uint>, IInitializable<GridStateCreateParams>, IDestroy
    {
        //public Action<GridChunk> chunkLoaded;

        //public Action<DataLayerEventArgs, GridState> layerChanged;
        public Action<GridState, DataLayer> layerAdded;

        public Action<GridState, string> layerRemoved;

        public uint Key => _instanceId;
        public string GridSettingsName => _gridSettings.Name;
        public GridSettings GridSettings => _gridSettings;
        //internal IEnumerable<GridChunk> LoadedChunks => _chunkEnv.Registry.Values;

        public Action OnDestroyAction { get; set; }

        public ReadOnlyCollection<DataLayer> DataLayers => _dataLayers.AsReadOnly();

        private uint _instanceId;
        private GridSettings _gridSettings;
        private ChunkStorageFabric _chunkStorageFabric;
        private List<DataLayer> _dataLayers = new();

        public void Destroy()
        {
            OnDestroyAction?.Invoke();

            _gridSettings.layerAdded -= OnLayerSettingsAdded;
            _gridSettings.layerRemoved -= OnLayerSettingsRemoved;
        }

        public void Init(GridStateCreateParams createParams, uint counter)
        {
            _instanceId = counter;
            _gridSettings = createParams.gridSettings;
            _chunkStorageFabric = createParams.chunkStorageFabric;

            Assert.IsNotNull( _gridSettings );
            Assert.IsNotNull( _chunkStorageFabric );

            //Init layers
            foreach (var layerSettings in _gridSettings.Settings.layers) {
                AddLayer( layerSettings );
            }
            _gridSettings.layerAdded += OnLayerSettingsAdded;
            _gridSettings.layerRemoved += OnLayerSettingsRemoved;
        }

        private DataLayer AddLayer(DataLayerSettings layerSettings)
        {
            if (layerSettings.layerType == LayerType.BlockLayer) {
                var chunkStorage = _chunkStorageFabric.GetChunkStorage( layerSettings, this );
                var blockLayer = new BlockLayer<BlockData>( layerSettings, chunkStorage );
                _dataLayers.Add( blockLayer );
                return blockLayer;
            } else {
                throw new LevelAPIException( $"Unknown layer type {layerSettings.layerType} in {layerSettings.tag}" );
            }
        }

        public DataLayer AddViewLayer(string tag, DataLayer mainLayer)
        {
            DataLayerSettings layerSettings = new( mainLayer.Settings );
            layerSettings.hasViewLayer = false;
            layerSettings.tag = tag;
            if (layerSettings.layerType == LayerType.BlockLayer) {
                var chunkStorage = _chunkStorageFabric.GetChunkStorage( layerSettings, this );
                var viewLayer = new BlockLayer<ClientViewData>( layerSettings, chunkStorage );
                return viewLayer;
            } else {
                throw new Exception( $"Unknown layer type {layerSettings.layerType}" );
            }
        }

        private void RemoveLayer(DataLayerSettings layerSettings)
        {
            var layer = GetLayer( layerSettings.tag );
            if (layer == null) {
                throw new LevelAPIException( $"Missing layer {layerSettings.tag}" );
            }
            _dataLayers.Remove( layer );
        }

        private void RemoveViewLayer()
        {
        }

        public DataLayer GetLayer(string tag)
        {
            return _dataLayers.SingleOrDefault( x => x.Tag == tag );
        }

        /// <summary>
        /// Установка
        /// </summary>
        /// <param name="layerTag"></param>
        /// <param name="chunkKey"></param>
        /// <param name="blockData"></param>
        /// <exception cref="LevelAPIException"></exception>
        public void AddBlock(string layerTag, ChunkDataKey chunkKey, object blockData)
        {
            var layer = GetLayer( layerTag );
            if (layer == null)
                throw new LevelAPIException( $"Grid state {Key}. Layer {layerTag} not found." );

            if (layer is BlockLayer<BlockData> blockLayer) {
                blockLayer.SetData( chunkKey, (BlockData)blockData );
            } else {
                throw new LevelAPIException( $"Unsupported block type {layer.GetType()}" );
            }
        }

        public void AddBlock<TData, TGlobalKey>(string layerTag, TGlobalKey key, TData data)
        {
            var layer = GetLayer( layerTag );
            if (layer == null) {
                throw new LevelAPIException( $"Missing layer {layerTag}" );
            }

            if (layer is ChunkLayer<TData, TGlobalKey> chunkLayer) {
                chunkLayer.SetData( key, data );
            } else {
                throw new LevelAPIException( $"Layer {layerTag} is not {nameof( ChunkLayer<TData, TGlobalKey> )}" );
            }
        }

        public static int BlockCoordToFlat(Vector3Int blockCoord, Vector3Int chunkSize)
        {
            return blockCoord.y * chunkSize.x * chunkSize.z + blockCoord.z * chunkSize.x + blockCoord.x;
        }

        public static Vector3Int FlatToBlockCoord(int flat, Vector3Int chunkSize)
        {
            int yLayerSize = chunkSize.x * chunkSize.z;
            int layerMod = flat % yLayerSize;
            return new Vector3Int( layerMod % chunkSize.x, flat / yLayerSize, layerMod / chunkSize.x );
        }

        public static int BlockCoordToFlatSafe(Vector3Int blockCoord, Vector3Int chunkSize)
        {
            if (blockCoord.x < 0 || blockCoord.x >= chunkSize.x
                    || blockCoord.y < 0 || blockCoord.y >= chunkSize.y
                    || blockCoord.z < 0 || blockCoord.z >= chunkSize.z) {
                throw new ArgumentException( $"Wrong block coordinate {blockCoord} with chunk size {chunkSize}" );
            } else {
                return BlockCoordToFlat( blockCoord, chunkSize );
            }
        }

        public static Vector3Int FlatToBlockCoordSafe(int flat, Vector3Int chunkSize)
        {
            int chunkSizeFlat = chunkSize.x * chunkSize.y * chunkSize.z;
            if (flat < 0 || flat >= chunkSizeFlat) {
                throw new ArgumentException( $"Wrong block index {flat} with chunk size {chunkSize}" );
            } else {
                return FlatToBlockCoord( flat, chunkSize );
            }
        }

        private void OnLayerSettingsRemoved(DataLayerSettings settings)
        {
            RemoveLayer( settings );
            layerRemoved?.Invoke( this, settings.tag );
        }

        private void OnLayerSettingsAdded(DataLayerSettings settings)
        {
            var layer = AddLayer( settings );
            layerAdded?.Invoke( this, layer );
        }

        internal void RemoveViewLayer(string tag)
        {
            throw new NotImplementedException();
        }
    }

    public struct GridStateCreateParams
    {
        public GridSettings gridSettings;
        public ChunkStorageFabric chunkStorageFabric;

        public GridStateCreateParams(GridSettings gridSettings, ChunkStorageFabric chunkStorageFabric)
        {
            this.gridSettings = gridSettings;
            this.chunkStorageFabric = chunkStorageFabric;
        }
    }

    public class GridStateRegistry : Registry<uint, GridState>
    { };

    public class GridStateFabric : Fabric<GridState, GridStateCreateParams>
    { };
}