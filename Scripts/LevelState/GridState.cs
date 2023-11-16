using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;

namespace Level
{
    /// <summary>
    /// Хранит текущее состояние пространственной сетки и её содержимое.
    /// Содержимое хранится в виде независимых слоёв, каждый из которых представляет свой тип данных.
    /// Слой может быть разбит на чанки. Чанки разных слоёв могут быть загружены независимо друг от друга.
    /// </summary>
    public class GridState : IHasKey<uint>, IInitializable<GridStateCreateParams>, IDestroy
    {
        //public Action<GridChunk> chunkLoaded;

        public Action<DataLayerEventArgs, GridState> layerChanged;

        public uint Key => _instanceId;
        public string GridSettingsName => _gridSettings.Name;
        public GridSettings GridSettings => _gridSettings;
        //internal IEnumerable<GridChunk> LoadedChunks => _chunkEnv.Registry.Values;

        public Action OnDestroyAction { get; set; }

        public ReadOnlyCollection<DataLayer> DataLayers => _dataLayers.AsReadOnly();

        private uint _instanceId;
        private GridSettings _gridSettings;

        //private DataLayerFabric _dataLayerFabric;
        //private TypeEnv<GridChunk, Vector3Int, GridChunkCreateParams, GridChunkFabric, GridChunkRegistry> _chunkEnv;
        private List<DataLayer> _dataLayers = new();

        private ChunkStorage _chunkStorage;

        public void Destroy()
        {
            OnDestroyAction?.Invoke();
        }

        public void Init(GridStateCreateParams value, uint counter)
        {
            _instanceId = counter;
            _gridSettings = value.gridSettings;
            //_dataLayerFabric = value.dataLayerFabric;

            Assert.IsNotNull( _gridSettings );
            //Assert.IsNotNull( _dataLayerFabric );

            //_chunkEnv = new();
        }

        //public GridChunk GetChunk(Vector3Int chunkCoord)
        //{
        //    GridChunk chunk;

        //    if (!_chunkEnv.Registry.Dict.TryGetValue( chunkCoord, out chunk )) {
        //        chunk = LoadChunk( chunkCoord );
        //    }
        //    return chunk;
        //}

        //private GridChunk LoadChunk(Vector3Int chunkCoord)
        //{
        //    //TODO User extensions for chunk load
        //    var chunkCreateParams = new GridChunkCreateParams( chunkCoord, _gridSettings.Settings.layers, _gridSettings, _dataLayerFabric, null );
        //    GridChunk chunk = _chunkEnv.Fabric.Create( chunkCreateParams );
        //    if (chunkLoaded != null) {
        //        chunkLoaded( chunk );
        //    }
        //    return chunk;
        //}

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