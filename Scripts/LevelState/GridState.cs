using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Level
{
    public class GridState : IHasKey<uint>, IInitializable<GridStateCreateParams>, IDestroy
    {
        public Action<GridChunk> chunkLoaded;

        public uint Key => _instanceId;
        public string GridSettingsName => _gridSettings.Name;
        public GridSettings GridSettings => _gridSettings;
        internal IEnumerable<GridChunk> LoadedChunks => _chunkEnv.Registry.Values;

        public Action OnDestroyAction { get; set; }

        private uint _instanceId;
        private GridSettings _gridSettings;
        private DataLayerFabric _dataLayerFabric;
        private TypeEnv<GridChunk, Vector3Int, GridChunkCreateParams, GridChunkFabric, GridChunkRegistry> _chunkEnv;

        public void Destroy()
        {
            OnDestroyAction?.Invoke();
        }

        public void Init(GridStateCreateParams value, uint counter)
        {
            _instanceId = counter;
            _gridSettings = value.gridSettings;
            _dataLayerFabric = value.dataLayerFabric;

            Assert.IsNotNull( _gridSettings );
            Assert.IsNotNull( _dataLayerFabric );

            _chunkEnv = new();
        }

        public GridChunk GetChunk(Vector3Int chunkCoord)
        {
            GridChunk chunk;

            if (!_chunkEnv.Registry.Dict.TryGetValue( chunkCoord, out chunk )) {
                chunk = LoadChunk( chunkCoord );
            }
            return chunk;
        }

        private GridChunk LoadChunk(Vector3Int chunkCoord)
        {
            //TODO User extensions for chunk load
            var chunkCreateParams = new GridChunkCreateParams( chunkCoord, _gridSettings.Settings.layers, _gridSettings, _dataLayerFabric, null );
            GridChunk chunk = _chunkEnv.Fabric.Create( chunkCreateParams );
            if (chunkLoaded != null) {
                chunkLoaded( chunk );
            }
            return chunk;
        }
    }

    public struct GridStateCreateParams
    {
        public GridSettings gridSettings;
        public DataLayerFabric dataLayerFabric;

        public GridStateCreateParams(GridSettings gridSettings, DataLayerFabric dataLayerFabric)
        {
            this.gridSettings = gridSettings;
            this.dataLayerFabric = dataLayerFabric;
        }
    }

    public class GridStateRegistry : Registry<uint, GridState>
    { };

    public class GridStateFabric : Fabric<GridState, GridStateCreateParams>
    { };

    //public interface IChunkSource
    //{
    //    /// <summary>
    //    /// Load or generate chunks data
    //    /// </summary>
    //    /// <param name="gridId"></param>
    //    /// <param name="chunkCoord"></param>
    //    /// <returns></returns>
    //    public List<SavedDataLayer> GetDataLayers(uint gridId, Vector3Int chunkCoord);
    //    public struct SavedDataLayer
    //    {
    //        public string name;
    //        public DataLayer dataLayer;
    //    }
    //}

    //public class DefaultChunkSource : IChunkSource
    //{
    //    private string _chunksFolder;

    //    public DefaultChunkSource(string chunksFolder)
    //    {
    //        _chunksFolder = chunksFolder;
    //    }

    //    public List<IChunkSource.SavedDataLayer> GetDataLayers(uint gridId, Vector3Int chunkCoord)
    //    {
    //        List<IChunkSource.SavedDataLayer> result = new();
    //        string chunkName = $"{gridId}.{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}";
    //        string chunkFile = JsonDataIO.FileFullName( _chunksFolder, chunkName );

    //        if (File.Exists( chunkFile )) {
    //            // Load existed chunk from file
    //            var chunkSerial = JsonDataIO.LoadData<ChunkSerializable>( _chunksFolder, chunkName );

    //            for(DataLayerSerializable layer in chunkSerial.dataLayer) {
    //                layer.layerType
    //            }
    //        } else {
    //            // Create empty data
    //        }
    //    }
    //}
}