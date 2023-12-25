using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Level.API;

using UnityEngine;

namespace Level.IO
{
    public interface ILevelLoader
    {
        void LoadFullContent(LevelAPI levelAPI, string levelPath = null);
    }

    public class FileLevelLoader : ILevelLoader
    {
        private string _filePath;
        private ChunkStorageFabric _chunkStorageFabric;
        private List<ChunkStorage> _chunkStorages = new();

        public FileLevelLoader(string filePath, ChunkStorageFabric chunkStorageFabric)
        {
            _filePath = filePath;

            _chunkStorageFabric = chunkStorageFabric;
            _chunkStorageFabric.created += OnChunkStorageCreated;
        }

        public void LoadFullContent(LevelAPI levelAPI, string levelPath)
        {
            //TODO Level/Server/Game settings
            //TODO Load Items

            string currentPath = levelPath ?? _filePath;

            //Load block prototypes (settings)
            LoadBlockProtos(levelAPI, currentPath);

            //Load grids settings
            LoadGridSettings(levelAPI, currentPath);

            //Load grid states (blocks, props, items, actors, etc.)
            LoadGridStatesHeaders(levelAPI, currentPath);

            LoadGridStatesBodies(levelAPI, currentPath);
        }

        private void OnChunkStorageCreated(ChunkStorage storage)
        {
            _chunkStorages.Add(storage);
        }

        public static void LoadBlockProtos(LevelAPI level, string path)
        {
            var collectionSerial = JsonDataIO.LoadData<ListWrapper<BlockProtoSerializable>>(path, LevelFileNames.FILE_BLOCK_PROTO);
            foreach (var blockSerial in collectionSerial.list) {
                blockSerial.Load(level.BlockProtoCollection);
            }
        }

        public static void LoadGridSettings(LevelAPI level, string path)
        {
            var collectionSerial = JsonDataIO.LoadData<ListWrapper<GridSettingsSerializable>>(path, LevelFileNames.FILE_GRID_SETTINGS);
            foreach (var gridSettingsSerial in collectionSerial.list) {
                gridSettingsSerial.Load(level.GridSettingsCollection);
            }
        }

        public static void LoadGridStatesHeaders(LevelAPI level, string path)
        {
            var collectionSerial = JsonDataIO.LoadData<ListWrapper<GridStateSerializable>>(path, LevelFileNames.FILE_GRID_STATE);
            foreach (var gridStateSerial in collectionSerial.list) {
                gridStateSerial.Load(level.GridStatesCollection, level.GridSettingsCollection);
            }
        }

        public static void LoadGridStatesBodies(LevelAPI level, string path)
        {
            foreach (var gridState in level.GridStatesCollection) {
                foreach (var dataLayer in gridState.DataLayers) {
                    string folder = path + "\\" + LevelFileNames.GetDataLayerSubFolder(dataLayer.Settings, gridState);

                    if (dataLayer.LayerType == LayerType.BlockLayer) {
                        var chunks = ReadChunksCoordsFromDir(folder);
                        var chunkLayer = (SimpleChunkLayer<BlockData>)dataLayer;
                        foreach (Vector3Int chunkCoord in chunks) {
                            LoadChunk(folder, chunkCoord, chunkLayer.GetChunkData(chunkCoord));
                        }
                    } else if (dataLayer.LayerType == LayerType.BigBlockLayer) {
                        var chunks = ReadChunksCoordsFromDir(folder);
                        var chunkLayer = (SimpleChunkLayer<BigBlockData>)dataLayer;
                        foreach (Vector3Int chunkCoord in chunks) {
                            LoadChunk(folder, chunkCoord, chunkLayer.GetChunkData(chunkCoord));
                        }
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public static List<Vector3Int> ReadChunksCoordsFromDir(string path)
        {
            List<Vector3Int> result = new();
            if (Directory.Exists(path)) {
                string[] files = Directory.GetFiles(path);
                var re = new Regex(LevelFileNames.CHUNK_COORD_REGEX);
                foreach (string filename in files) {
                    var match = re.Match(filename);
                    if (match.Success) {
                        Vector3Int blockCoord = new(
                            int.Parse(match.Groups[2].Value),
                            int.Parse(match.Groups[3].Value),
                            int.Parse(match.Groups[4].Value));
                        result.Add(blockCoord);
                    }
                }
            }
            return result;
        }

        public static void LoadChunk<T>(string dataLayerPath, Vector3Int coord, DataLayerContent<T> content)
        {
            string fileName = LevelFileNames.GetChunkFile(coord);
            if (content is DataLayerStaticContent<BlockData> bdContent) {
                var serial = JsonDataIO.LoadData<BlockChunkConvertSerializable>(dataLayerPath, fileName);
                serial.Load(bdContent);
            } else if (content is DataLayerDynamicContent<BigBlockData> bbdContent) {
                var serial = JsonDataIO.LoadData<BigBlockChunkContentSerializable>(dataLayerPath, fileName);
                serial.Load(bbdContent);
            } else {
                throw new NotImplementedException();
            }
        }
    }
}