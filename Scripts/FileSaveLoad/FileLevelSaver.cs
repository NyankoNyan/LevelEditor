using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Level.API;

using UnityEngine;

namespace Level.IO
{
    public interface ILevelSave
    {
        void SaveFullContent(LevelAPI level, string savePath = null);
    }

    internal static class LevelFileNames
    {
        public static Encoding ENCODING => Encoding.UTF8;
        public const string JSON_EXTENSION = ".json";
        public const string FILE_BLOCK_PROTO = "block_proto";
        public const string FILE_GRID_SETTINGS = "grid_settings";
        public const string FILE_GRID_STATE = "grid_state";
        public const string DIR_CHUNKS = "chunks";
        public const string LAYER_BLOCKS = "blocks";
        public const string LAYER_BIG_BLOCKS = "big_blocks";

        public const string CHUNK_COORD_REGEX = @".*\\((-?\d+)_(-?\d+)_(-?\d+).json)$";

        public static string GetChunkFile(Vector3Int chunkCoord) => $"{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}";

        public static string GetChunkFileFull(Vector3Int chunkCoord) => $"{GetChunkFile(chunkCoord)}.json";

        public static string GetDataLayerSubFolder(DataLayerSettings dataLayerSettings, GridState gridState)
        {
            string gridFolder = gridState.Key.ToString();
            if (dataLayerSettings.layerType == LayerType.BlockLayer) {
                return gridFolder + "\\" + LevelFileNames.LAYER_BLOCKS + '_' + dataLayerSettings.tag;
            } else if (dataLayerSettings.layerType == LayerType.BigBlockLayer) {
                return gridFolder + "\\" + LevelFileNames.LAYER_BIG_BLOCKS + '_' + dataLayerSettings.tag;
            } else {
                throw new Exception($"Unknown layer type {dataLayerSettings.layerType}");
            }
        }
    }

    public class FileLevelSaver : ILevelSave
    {
        private string _filePath;
        private bool _prettyPrint;
        private ChunkStorageFabric _chunkStorageFabric;
        private List<ChunkStorage> _chunkStorages = new();

        public FileLevelSaver(string filePath, bool prettyPrint, ChunkStorageFabric chunkStorageFabric)
        {
            _filePath = filePath;
            _prettyPrint = prettyPrint;
            _chunkStorageFabric = chunkStorageFabric;
            _chunkStorageFabric.created += OnChunkStorageCreated;
        }

        private void OnChunkStorageCreated(ChunkStorage storage)
        {
            _chunkStorages.Add(storage);
        }

        public void SaveFullContent(LevelAPI level, string savePath)
        {
            string currentSavePath = savePath ?? _filePath;

            CheckDirectory(currentSavePath);
            SaveBlockProtos(level, currentSavePath, _prettyPrint);
            SaveGridSettings(level, currentSavePath, _prettyPrint);
            SaveGridStatesHeaders(level, currentSavePath, _prettyPrint);
            SaveGridStatesBodies(level, currentSavePath, _prettyPrint);
        }

        private static void CheckDirectory(string path)
        {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static void SaveBlockProtos(LevelAPI level, string levelSavePath, bool prettyPrint = false)
        {
            JsonDataIO.SaveData(new ListWrapper<BlockProtoSerializable>(level.BlockProtoCollection.Select(x => (BlockProtoSerializable)x).ToArray()), levelSavePath, LevelFileNames.FILE_BLOCK_PROTO, prettyPrint);
        }

        public static void SaveGridSettings(LevelAPI level, string levelSavePath, bool prettyPrint = false)
        {
            JsonDataIO.SaveData(new ListWrapper<GridSettingsSerializable>(level.GridSettingsCollection.Select(x => (GridSettingsSerializable)x).ToArray()), levelSavePath, LevelFileNames.FILE_GRID_SETTINGS, prettyPrint);
        }

        public static void SaveGridStatesHeaders(LevelAPI level, string levelSavePath, bool prettyPrint = false)
        {
            JsonDataIO.SaveData(new ListWrapper<GridStateSerializable>(level.GridStatesCollection.Select(x => (GridStateSerializable)x).ToArray()), levelSavePath, LevelFileNames.FILE_GRID_STATE, prettyPrint);
        }

        public static void SaveGridStatesBodies(LevelAPI level, string levelSavePath, bool prettyPrint = false)
        {
            foreach (var gridState in level.GridStatesCollection) {
                SaveGridStateBody(gridState, levelSavePath, prettyPrint);
            }
        }

        public static void SaveGridStateBody(GridState gridState, string levelSavePath, bool prettyPrint = false)
        {
            foreach (var dataLayer in gridState.DataLayers) {
                SaveDataLayer(dataLayer, gridState, levelSavePath, prettyPrint);
            }
        }

        public static void SaveDataLayer(DataLayer dataLayer, GridState gridState, string levelSavePath, bool prettyPrint = false)
        {
            string dataLayerSavePath = levelSavePath + "\\" + LevelFileNames.GetDataLayerSubFolder(dataLayer.Settings, gridState);
            CheckDirectory(dataLayerSavePath);
            if (dataLayer is SimpleChunkLayer<BlockData> blockLayer) {
                foreach (Vector3Int chunkCoord in blockLayer.ExistedChunks) {
                    var chunkData = blockLayer.GetChunkData(chunkCoord);
                    SaveChunk(dataLayerSavePath, chunkCoord, chunkData, prettyPrint);
                }
            } else if (dataLayer is SimpleChunkLayer<BigBlockData> bigBlockLayer) {
                foreach (Vector3Int chunkCoord in bigBlockLayer.ExistedChunks) {
                    var chunkData = bigBlockLayer.GetChunkData(chunkCoord);
                    SaveChunk(dataLayerSavePath, chunkCoord, chunkData, prettyPrint);
                }
            } else {
                throw new NotImplementedException();
            }
        }

        public static string SaveChunk<T>(string dataLayerSavePath, Vector3Int coord, DataLayerContent<T> currentData, bool prettyPrint = false)
        {
            string filename = $"{coord.x}_{coord.y}_{coord.z}";
            if (currentData is DataLayerStaticContent<BlockData> bdContent) {
                var serializable = (BlockChunkConvertSerializable)bdContent;
                JsonDataIO.SaveData(serializable, dataLayerSavePath, filename, prettyPrint);
            } else if (currentData is DataLayerDynamicContent<BigBlockData> bbdContent) {
                var serializable = (BigBlockChunkContentSerializable)bbdContent;
                JsonDataIO.SaveData(serializable, dataLayerSavePath, filename, prettyPrint);
            } else {
                throw new NotImplementedException();
            }
            return filename;
        }
    }
}