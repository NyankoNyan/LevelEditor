using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    public struct GridChunkCreateParams
    {
        public Vector3Int coord;
        public IEnumerable<DataLayerSettings> layerSettings;
        public GridSettings gridSettings;
        public DataLayerFabric dataLayerFabric;
        public IEnumerable<DataLayer> completeDataLayers;

        public GridChunkCreateParams(
            Vector3Int coord,
            IEnumerable<DataLayerSettings> layerSettings,
            GridSettings gridSettings,
            DataLayerFabric dataLayerFabric,
            IEnumerable<DataLayer> completeDataLayers)
        {
            this.coord = coord;
            this.layerSettings = layerSettings;
            this.gridSettings = gridSettings;
            this.dataLayerFabric = dataLayerFabric;
            this.completeDataLayers = completeDataLayers;
        }
    }

    public class GridChunk : IHasKey<Vector3Int>, IInitializable<GridChunkCreateParams>, IDestroy
    {
        public UnityAction<DataLayer> layerAdded;
        public UnityAction<string> layerRemoved;

        private Vector3Int _id;
        private DataLayerFabric _dataLayerFabric;
        private GridSettings _gridSettings;
        private List<DataLayer> _dataLayers = new();
        private int _chunkFlatSize;

        public Vector3Int Key => _id;
        public IEnumerable<DataLayer> Layers => _dataLayers;

        public UnityAction OnDestroyAction { get; set; }

        public void Destroy()
        {
            OnDestroyAction?.Invoke();
        }

        public void Init(GridChunkCreateParams createParams, uint counter = 0)
        {
            _id = createParams.coord;
            _dataLayerFabric = createParams.dataLayerFabric;
            _gridSettings = createParams.gridSettings;

            Vector3Int chunkSize = createParams.gridSettings.Settings.chunkSize;
            _chunkFlatSize = createParams.gridSettings.ChunkSizeFlat;

            DataLayer[] tempDataLayers = null;
            if (createParams.completeDataLayers != null) {
                tempDataLayers = createParams.completeDataLayers.ToArray();
            }

            // Data layers initialization
            foreach (var layerSettings in createParams.layerSettings) {
                bool presetLayerFound = false;
                if (tempDataLayers != null) {
                    DataLayer dataLayer = tempDataLayers.FirstOrDefault( x =>
                         x.LayerType == layerSettings.layerType
                         && x.Tag == layerSettings.tag );
                    if (dataLayer != null) {
                        AddLayer( layerSettings, dataLayer );
                        presetLayerFound = true;
                    }
                }
                if (!presetLayerFound) {
                    CreateLayer( layerSettings );
                }
            }

            // Event handlers
            UnityAction<DataLayerSettings> layerAdded = (layerSettings) => {
                DataLayer dataLayer = CreateLayer( layerSettings );
                this.layerAdded?.Invoke( dataLayer );
            };

            UnityAction<DataLayerSettings> layerRemoved = (layerSettings) => {
                int ri = _dataLayers.FindIndex( x => x.Tag == layerSettings.tag );
                _dataLayers.RemoveAt( ri );
                this.layerRemoved?.Invoke( layerSettings.tag );
            };

            _gridSettings.layerAdded += layerAdded;
            _gridSettings.layerRemoved += layerRemoved;

            UnityAction onDestroy = null;
            onDestroy = () => {
                _gridSettings.layerAdded -= layerAdded;
                _gridSettings.layerRemoved -= layerRemoved;
                OnDestroyAction -= onDestroy;
            };
            OnDestroyAction += onDestroy;
        }

        private DataLayer CreateLayer(DataLayerSettings layerSettings)
        {
            DataLayer dataLayer = _dataLayerFabric.Create( layerSettings.layerType, layerSettings.tag, _chunkFlatSize );
            _dataLayers.Add( dataLayer );
            return dataLayer;
        }

        private void AddLayer(DataLayerSettings layerSettings, DataLayer dataLayer)
        {
            if (layerSettings.layerType != dataLayer.LayerType
                || layerSettings.tag != dataLayer.Tag) {
                throw new ArgumentException( $"Wrong layer type or tag" );
            }

            switch (layerSettings.layerType) {
                case LayerType.BlockLayer:
                    BlockLayer blockLayer = dataLayer as BlockLayer;
                    if (blockLayer == null) {
                        throw new ArgumentException( "This isn't block layer's data" );
                    }
                    AddBlockLayer( blockLayer );
                    break;

                default:
                    throw new ArgumentException( $"Unknown layer type {layerSettings.layerType}" );
            }
        }

        private void AddBlockLayer(BlockLayer blockLayer)
        {
            if (blockLayer.Data.Length != _gridSettings.ChunkSizeFlat) {
                throw new ArgumentException( $"Wrong layer size {blockLayer.Data.Length} (correct {_gridSettings.ChunkSizeFlat})" );
            }
            _dataLayers.Add( blockLayer );
        }

        public DataLayer GetLayer(LayerType layerType, string tag)
        {
            try {
                return _dataLayers.Single( x => x.LayerType == layerType && x.Tag == tag );
            } catch (Exception e) {
                throw new ArgumentException( $"Unknown layer {layerType} - {tag}", e );
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
    }

    public class GridChunkFabric : Fabric<GridChunk, GridChunkCreateParams>
    { }

    public class GridChunkRegistry : Registry<Vector3Int, GridChunk>
    { }
}