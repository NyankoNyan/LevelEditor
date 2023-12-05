using Level.API;

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
        private ChunkStorageFabric _chunkStorageFabric;
        private List<ChunkStorage> _chunkStorages = new();

        public FileLevelLoader(string filePath, ChunkStorageFabric chunkStorageFabric)
        {
            _filePath = filePath;

            _chunkStorageFabric = chunkStorageFabric;
            _chunkStorageFabric.created += OnChunkStorageCreated;
        }

        public void LoadFullContent(LevelAPI levelAPI)
        {
            //TODO Level/Server/Game settings 
            //TODO Load Items

            //Load block prototypes (settings)
            var blPrSerials = LoadData<ListWrapper<BlockProtoSerializable>>(LevelFileConsts.FILE_BLOCK_PROTO);
            foreach (var blPrSerial in blPrSerials.list) {
                blPrSerial.Load(levelAPI.BlockProtoCollection);
            }

            //Load grids settings
            var grSetSerials = LoadData<ListWrapper<GridSettingsSerializable>>(LevelFileConsts.FILE_GRID_SETTINGS);
            foreach (var grSetSerial in grSetSerials.list) {
                grSetSerial.Load(levelAPI.GridSettingsCollection);
            }

            //Load grid states (blocks, props, items, actors, etc.)
            var grStateSerials = LoadData<ListWrapper<GridStateSerializable>>(LevelFileConsts.FILE_GRID_STATE);
            foreach (var grStateSerial in grStateSerials.list) {
                grStateSerial.Load(levelAPI.GridStatesCollection, levelAPI.GridSettingsCollection);
            }

            foreach (var chunkStorage in _chunkStorages) {
                foreach (Vector3Int chunkCoord in chunkStorage.GetLoadedChunks()) {
                    chunkStorage.LoadChunk(chunkCoord);
                }
            }
        }

        private void OnChunkStorageCreated(ChunkStorage storage)
        {
            _chunkStorages.Add(storage);
        }

        private T LoadData<T>(string file)
        {
            return JsonDataIO.LoadData<T>(_filePath, file);
        }
    }
}