using Level.API;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Level.IO
{
    public interface ILevelLoader
    {
        void LoadFullContent(LevelAPI levelAPI);
    }

    public class FileLevelLoader : ILevelLoader
    {
        private string _filePath;

        public FileLevelLoader(string filePath)
        {
            _filePath = filePath;
        }

        public void LoadFullContent(LevelAPI levelAPI)
        {
            var blPrSerials = LoadData<ListWrapper<BlockProtoSerializable>>( LevelFileConsts.FILE_BLOCK_PROTO );
            foreach (var blPrSerial in blPrSerials.list) {
                blPrSerial.Load( levelAPI.BlockProtoCollection );
            }

            var grSetSerials = LoadData<ListWrapper<GridSettingsSerializable>>( LevelFileConsts.FILE_GRID_SETTINGS );
            foreach (var grSetSerial in grSetSerials.list) {
                grSetSerial.Load( levelAPI.GridSettingsCollection );
            }

            var grStateSerials = LoadData<ListWrapper<GridStateSerializable>>( LevelFileConsts.FILE_GRID_STATE );
            foreach (var grStateSerial in grStateSerials.list) {
                grStateSerial.Load( levelAPI.GridStatesCollection, levelAPI.GridSettingsCollection );
            }

            LoadChunks( levelAPI.GridStatesCollection );
        }

        private void LoadChunks(GridStatesCollection gridStatesAPI)
        {
            string[] chunkFiles = Directory.GetFiles( $"{_filePath}/{LevelFileConsts.DIR_CHUNKS}" );

            var regex = new Regex( @"(\d+)\.(-?\d+)_(-?\d+)_(-?\d+)\.json" );

            foreach (string chunkFile in chunkFiles) {
                MatchCollection matches = regex.Matches( chunkFile );
                if (matches.Count == 0) {
                    Debug.LogError( $"Chunk file name {chunkFile} don't match with naming rule" );
                } else {
                    // Group[0] is full match
                    uint gridId = uint.Parse( matches[0].Groups[1].Value );
                    int x = int.Parse( matches[0].Groups[2].Value );
                    int y = int.Parse( matches[0].Groups[3].Value );
                    int z = int.Parse( matches[0].Groups[4].Value );

                    string subPath = $"{LevelFileConsts.DIR_CHUNKS}/{gridId}.{x}_{y}_{z}";
                    var chunkSerial = LoadData<ChunkSerializable>( subPath );

                    var gridState = gridStatesAPI[gridId];
                    if (gridState == null) {
                        Debug.LogError( $"Missing grid with id {gridId}" );
                    } else {
                        chunkSerial.Load( gridState );
                    }
                }
            }
        }

        private T LoadData<T>(string file)
        {
            return JsonDataIO.LoadData<T>( _filePath, file );
        }
    }

    public static class JsonDataIO
    {
        /// <summary>
        /// Loaded data from json file. Data must be serializable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder">Absolute or relative folder</param>
        /// <param name="file">File name without extension</param>
        /// <returns></returns>
        public static T LoadData<T>(string folder, string file)
        {
            string fullPath = FileFullName( folder, file );
            string json = File.ReadAllText( fullPath, LevelFileConsts.ENCODING );
            return JsonUtility.FromJson<T>( json );
        }

        /// <summary>
        /// Save data to json file. Data must be serializable.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="folder">Absolute or relative folder</param>
        /// <param name="file">File name without extension</param>
        /// <param name="prettyPrint">When true, file will be human readable</param>
        public static void SaveData(object obj, string folder, string file, bool prettyPrint)
        {
            string json = JsonUtility.ToJson( obj, prettyPrint );
            string fullPath = FileFullName( folder, file );
            File.WriteAllText( fullPath, json, LevelFileConsts.ENCODING );
        }

        public static string FileFullName(string folder, string file)
        {
            return $"{folder}/{file}{LevelFileConsts.JSON_EXTENSION}";
        }
    }
}