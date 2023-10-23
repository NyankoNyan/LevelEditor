using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Level.IO
{
    public interface ILevelSave
    {
        void SaveFullContent(
            BlockProtoRegistry blockProtoRegistry,
            GridSettingsRegistry gridSettingsRegistry,
            GridStateRegistry gridStateRegistry);
    }

    internal static class LevelFileConsts
    {
        public static Encoding ENCODING => Encoding.UTF8;
        public const string JSON_EXTENSION = ".json";
        public const string FILE_BLOCK_PROTO = "block_proto";
        public const string FILE_GRID_SETTINGS = "grid_settings";
        public const string FILE_GRID_STATE = "grid_state";
        public const string DIR_CHUNKS = "chunks";
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

        public FileLevelSaver(string filePath, bool prettyPrint)
        {
            _filePath = filePath;
            _prettyPrint = prettyPrint;
        }

        public void SaveFullContent(
            BlockProtoRegistry blockProtoRegistry,
            GridSettingsRegistry gridSettingsRegistry,
            GridStateRegistry gridStateRegistry)
        {
            //TODO Rewrite with API
            SaveData( new ListWrapper<BlockProtoSerializable>( blockProtoRegistry.Values.Select( x => (BlockProtoSerializable)x ).ToArray() ), LevelFileConsts.FILE_BLOCK_PROTO );
            SaveData( new ListWrapper<GridSettingsSerializable>( gridSettingsRegistry.Values.Select( x => (GridSettingsSerializable)x ).ToArray() ), LevelFileConsts.FILE_GRID_SETTINGS );
            SaveData( new ListWrapper<GridStateSerializable>( gridStateRegistry.Values.Select( x => (GridStateSerializable)x ).ToArray() ), LevelFileConsts.FILE_GRID_STATE );
            SaveChunks( gridStateRegistry );
        }

        private void SaveData(object obj, string file)
        {
            JsonDataIO.SaveData( obj, _filePath, file, _prettyPrint );
        }

        private void SaveChunks(GridStateRegistry gridStateRegistry)
        {
            CheckDirectory( LevelFileConsts.DIR_CHUNKS );
            foreach (var gridState in gridStateRegistry.Values) {
                foreach (var gridChunk in gridState.LoadedChunks) {
                    string subPath = $"{LevelFileConsts.DIR_CHUNKS}/{gridState.Key}.{gridChunk.Key.x}_{gridChunk.Key.y}_{gridChunk.Key.z}";
                    SaveData( (ChunkSerializable)gridChunk, subPath );
                }
            }
        }

        private void CheckDirectory(string path)
        {
            string fullPath = $"{_filePath}/{path}";
            if (!Directory.Exists( fullPath )) {
                Directory.CreateDirectory( fullPath );
            }
        }
    }
}