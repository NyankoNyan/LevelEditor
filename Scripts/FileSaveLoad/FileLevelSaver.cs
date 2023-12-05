using System;
using System.IO;
using System.Linq;
using System.Text;

using Level.API;

using UnityEngine;

namespace Level.IO
{
    public interface ILevelSave
    {
        void SaveFullContent(LevelAPI level);
        void SetChunkStorageFabric(ChunkStorageFabric chunkStorageFabric);
    }

    internal static class LevelFileConsts
    {
        public static Encoding ENCODING => Encoding.UTF8;
        public const string JSON_EXTENSION = ".json";
        public const string FILE_BLOCK_PROTO = "block_proto";
        public const string FILE_GRID_SETTINGS = "grid_settings";
        public const string FILE_GRID_STATE = "grid_state";
        public const string DIR_CHUNKS = "chunks";
        public const string LAYER_BLOCKS = "blocks";
        public const string LAYER_BIG_BLOCKS = "big_blocks";
    }

    [Serializable]
    internal class ListWrapper<T>
    {
        public T[] list;

        public ListWrapper(T[] list)
        {
            this.list = list;
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


        public void SaveFullContent(LevelAPI level)
        {
            CheckDirectory(_filePath);
            SaveData(new ListWrapper<BlockProtoSerializable>(level.BlockProtoCollection.Select(x => (BlockProtoSerializable)x).ToArray()), LevelFileConsts.FILE_BLOCK_PROTO);
            SaveData(new ListWrapper<GridSettingsSerializable>(level.GridSettingsCollection.Select(x => (GridSettingsSerializable)x).ToArray()), LevelFileConsts.FILE_GRID_SETTINGS);
            SaveData(new ListWrapper<GridStateSerializable>(level.GridStatesCollection.Select(x => (GridStateSerializable)x).ToArray()), LevelFileConsts.FILE_GRID_STATE);

            foreach(var chunkStorage in _chunkStorages){
                foreach(Vector3Int chunkCoord in chunkStorage.GetLoadedChunks()){
                    //TODO Save chunk
                    // chunkStorage.SaveChunk(chunkCoord);
                }
            }
        }

        private void SaveData(object obj, string file)
        {
            JsonDataIO.SaveData(obj, _filePath, file, _prettyPrint);
        }

        private void CheckDirectory(string path)
        {
            string fullPath = $"{_filePath}/{path}";
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }
        }
    }
}