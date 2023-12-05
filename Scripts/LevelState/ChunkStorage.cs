using Level.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

namespace Level
{
    /// <summary>
    /// Долговременное хранилище чанков.
    /// </summary>
    public abstract class ChunkStorage
    {
        protected HashSet<Vector3Int> _loadedChunks = new();
        public abstract object LoadChunk(Vector3Int coord);
        public abstract void SaveChunk(Vector3Int coord, object currentData);
        public abstract Vector3Int[] GetExistedChunks();
        public Vector3Int[] GetLoadedChunks()
        {
            return _loadedChunks.ToArray();
        }
    }

    public abstract class ChunkStorageFabric
    {
        public Action<ChunkStorage> created;
        public abstract ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings, GridState gridState);
    }

    public class MockChunkStorageFabric : ChunkStorageFabric
    {
        private SimpleBlockChunkStorage<BlockData> _blockStorage;
        private DynamicChunkStorage<BigBlockData> _bigBlockStorage;

        public override ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings, GridState gridState)
        {
            switch (dataLayerSettings.layerType) {
                case LayerType.BlockLayer:
                    if (_blockStorage == null) {
                        _blockStorage = new SimpleBlockChunkStorage<BlockData>(dataLayerSettings.chunkSize);
                    }
                    return _blockStorage;
                case LayerType.BigBlockLayer:
                    if (_bigBlockStorage == null) {
                        _bigBlockStorage = new DynamicChunkStorage<BigBlockData>();
                    }
                    return _bigBlockStorage;
                default:
                    throw new Exception();
            }
        }
    }

    public class DynamicChunkStorage<TData> : ChunkStorage
    {
        public override Vector3Int[] GetExistedChunks()
        {
            return new Vector3Int[0];
        }

        public override object LoadChunk(Vector3Int coord)
        {
            return new DataLayerDynamicContent<TData>();
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
        }
    }

    public class SimpleBlockChunkStorage<TData> : ChunkStorage
    {
        private Vector3Int _size;

        public SimpleBlockChunkStorage(Vector3Int size)
        {
            _size = size;
        }

        public override Vector3Int[] GetExistedChunks()
        {
            return new Vector3Int[0];
        }

        public override object LoadChunk(Vector3Int coord)
        {
            int flatSize = _size.x * _size.y * _size.z;
            DataLayerStaticContent<TData> content = new(flatSize);
            return content;
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
        }
    }

    public class FileChunkStorageFabric : ChunkStorageFabric
    {
        private string _folder;

        public FileChunkStorageFabric(string folder)
        {
            _folder = folder;
        }

        public override ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings, GridState gridState)
        {
            string gridFolder = _folder + "\\" + gridState.Key;
            if (dataLayerSettings.layerType == LayerType.BlockLayer) {
                string folder = gridFolder + "\\" + LevelFileConsts.LAYER_BLOCKS + '_' + dataLayerSettings.tag;
                int flatSize = gridState.GridSettings.ChunkSize.x * gridState.GridSettings.ChunkSize.y * gridState.GridSettings.ChunkSize.z;
                var blockStorage = new FileBlockChunkStorage<BlockData>(folder, flatSize);
                created?.Invoke(blockStorage);
                return blockStorage;
            } else if (dataLayerSettings.layerType == LayerType.BigBlockLayer) {
                string folder = gridFolder + "\\" + LevelFileConsts.LAYER_BIG_BLOCKS + '_' + dataLayerSettings.tag;
                var blockStorage = new FileDynamicChunkStorage<BigBlockData>(folder);
                created.Invoke(blockStorage);
                return blockStorage;
            } else {
                throw new Exception();
            }
        }
    }

    public class FileBlockChunkStorage<TData> : ChunkStorage
    {
        private string _directory;
        private int _chunkDataSize;
        private Dictionary<Vector3Int, string> _fileMap = new();

        private bool _existedInitialized;


        public FileBlockChunkStorage(string directory, int chunkDataSize)
        {
            _directory = directory;
            _chunkDataSize = chunkDataSize;
        }

        public override Vector3Int[] GetExistedChunks()
        {
            if (!_existedInitialized) {
                CollectFilesNames();
                _existedInitialized = true;
            }
            return _fileMap.Keys.ToArray();
        }

        public override object LoadChunk(Vector3Int coord)
        {
            DataLayerStaticContent<TData> content = new(_chunkDataSize);
            if (_fileMap.TryGetValue(coord, out string filename)) {
                if (content is DataLayerStaticContent<BlockData> blockContent) {
                    var serializable = JsonDataIO.LoadData<BlockChunkConvertSerializable>(_directory, filename);
                    serializable.Load(blockContent);
                    _loadedChunks.Add(coord);
                }
            }
            return content;
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
            var content = currentData as DataLayerStaticContent<BlockData>;
            var serializable = (BlockChunkConvertSerializable)content;
            string filename = $"{coord.x}_{coord.y}_{coord.z}.json";
            JsonDataIO.SaveData(serializable, _directory, filename, false);
            if (!_fileMap.ContainsKey(coord)) {
                _fileMap.Add(coord, filename);
            }
        }

        private void CollectFilesNames()
        {
            if (Directory.Exists(_directory)) {
                string[] files = Directory.GetFiles(_directory);
                var re = new Regex(@".*\\?((\d+)_(\d+)_(\d+).json)$");
                foreach (string filename in files) {
                    var match = re.Match(filename);
                    if (match.Success) {
                        Vector3Int blockCoord = new(
                            int.Parse(match.Groups[2].Value),
                            int.Parse(match.Groups[3].Value),
                            int.Parse(match.Groups[4].Value));
                        _fileMap.Add(blockCoord, match.Groups[1].Value);
                    }
                }
            }
        }
    }

    public class FileDynamicChunkStorage<TData> : ChunkStorage
    {
        private string _directory;
        private Dictionary<Vector3Int, string> _fileMap = new();

        public FileDynamicChunkStorage(string directory)
        {
            _directory = directory;
        }

        public override Vector3Int[] GetExistedChunks()
        {
            return _fileMap.Keys.ToArray();
        }

        public override object LoadChunk(Vector3Int coord)
        {
            DataLayerDynamicContent<TData> content = new();
            if (_fileMap.TryGetValue(coord, out string filename)) {
                if (content is DataLayerDynamicContent<BigBlockData> blockContent) {
                    var serializable = JsonDataIO.LoadData<BigBlockChunkContentSerializable>(_directory, filename);
                    serializable.Load(blockContent);
                }
            }
            return content;
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
            var content = currentData as DataLayerDynamicContent<BigBlockData>;
            var serializable = (BigBlockChunkContentSerializable)content;
            string filename = $"{coord.x}_{coord.y}_{coord.z}.json";
            JsonDataIO.SaveData(serializable, _directory, filename, false);
        }

    }
}