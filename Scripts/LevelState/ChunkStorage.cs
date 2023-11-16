﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using Level.IO;
using RuntimeEditTools;
using UnityEngine;

namespace Level
{
    /// <summary>
    /// Долговременное хранилище чанков.
    /// </summary>
    public abstract class ChunkStorage
    {
        public abstract object LoadChunk(Vector3Int coord);

        public abstract void SaveChunk(Vector3Int coord, object currentData);

        public abstract Vector3Int[] GetExistedChunks();
    }

    public abstract class ChunkStorageFabric
    {
        public abstract ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings, GridState gridState);
    }

    public class MockChunkStorageFabric : ChunkStorageFabric
    {
        private MockBlockChunkStorage _blockStorage;

        public override ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings, GridState gridState)
        {
            switch (dataLayerSettings.layerType) {
                case LayerType.BlockLayer:
                    if (_blockStorage == null) {
                        _blockStorage = new MockBlockChunkStorage( dataLayerSettings.chunkSize );
                    }
                    return _blockStorage;

                default:
                    throw new Exception();
            }
        }
    }

    public class MockBlockChunkStorage : ChunkStorage
    {
        private Vector3Int _size;

        public MockBlockChunkStorage(Vector3Int size)
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
            DataLayerStaticContent<BlockData> content = new( (uint)flatSize );
            return content;
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
        }
    }

    public class FileChunkStorageFabric : ChunkStorageFabric
    {
        string _folder;

        public FileChunkStorageFabric(string folder){
            _folder = folder;
        }

        public override ChunkStorage GetChunkStorage(DataLayerSettings dataLayerSettings, GridState gridState)
        {
            if(dataLayerSettings.layerType == LayerType.BlockLayer){
                string gridFolder = _folder + "\\" + gridState.Key;
                string folder = gridFolder + "\\" + LevelFileConsts.LAYER_BLOCKS + '_' + dataLayerSettings.tag;
                var blockStorage=new FileBlockChunkStorage<BlockData>(folder, gridState.GridSettings.ChunkSize);
                return blockStorage;
            }else{
                throw new Exception();
            }
        }
    }

    public class FileBlockChunkStorage<TData> : ChunkStorage
    {
        string _directory;
        private uint _chunkDataSize;
        Dictionary<Vector3Int, string> _fileMap = new();

        public FileBlockChunkStorage(string directory, uint chunkDataSize){
            _directory = directory;
            _chunkDataSize = chunkDataSize;
        }

        public override Vector3Int[] GetExistedChunks()
        {
            return _fileMap.Keys.ToArray();
        }

        public override object LoadChunk(Vector3Int coord)
        {
            DataLayerStaticContent<TData> content = new(_chunkDataSize);
            if(_fileMap.TryGetValue(coord, out string filename)){
                if(content is DataLayerStaticContent<BlockData> blockContent){
                    var serializable = JsonDataIO.LoadData<BlockChunkContertSerializable>( _directory, filename );
                    serializable.Load(blockContent);
                }
            }
            return content;
        }

        public override void SaveChunk(Vector3Int coord, object currentData)
        {
            var content = currentData as DataLayerStaticContent<BlockData>;
            var serializable = (BlockChunkContertSerializable)content;
            string filename = $"{coord.x}_{coord.y}_{coord.z}.json";
            JsonDataIO.SaveData( serializable, _directory, filename, false );
        }

        private void CollectFilesNames(){
            if(Directory.Exists(_directory)){
                string[] files = Directory.GetFiles(_directory);
                var re = new Regex(@".*\\?((\d+)_(\d+)_(\d+).json)$");
                foreach(string filename in files){
                    var match = re.Match(filename);
                    if(match.Success){
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
}