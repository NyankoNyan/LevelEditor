using System;

using Level.IO;

using UnityEngine;

namespace Level.API
{
    public class LevelAPIException : Exception
    {
        public LevelAPIException(string msg) : base(msg)
        {
        }
    }

    public class LevelAPIFabric
    {
        public LevelAPI Create(LevelSettings gameSettings = default)
        {
            return new LevelAPI(gameSettings);
        }
    }

    public enum ChunkStorageStrategy
    {
        DontSave = 0,
        AllTogether = 1,
        DynamicSaveLoad = 2
    }

    [Serializable]
    public struct LevelSettings
    {
        public ChunkStorageStrategy chunkStorageStrategy;
        public string levelStoreURI;
        public string name;
    }

    /// <summary>
    /// Предоставляет доступ к API уровня в целом и отдельным его компонентам.
    /// </summary>
    public class LevelAPI
    {
        private BlockProtoCollection _blockProtoCollection;

        private GridStatesCollection _gridStatesCollection;

        private GridSettingsCollection _gridSettingsCollection;

        private ChunkStorageFabric _chunkStorageFabric;

        private UserManager _userManager;
        private LevelSettings _settings;
        private ILevelLoader _levelLoader;
        private ILevelSave _levelSaver;

        internal LevelAPI(LevelSettings settings)
        {
            _settings = settings;

            switch (_settings.chunkStorageStrategy) {
                case ChunkStorageStrategy.DontSave:
                    _chunkStorageFabric = new MockChunkStorageFabric();
                    break;

                case ChunkStorageStrategy.AllTogether:
                case ChunkStorageStrategy.DynamicSaveLoad:
                    _chunkStorageFabric = new FileChunkStorageFabric(_settings.levelStoreURI + "\\" + LevelFileNames.DIR_CHUNKS);
                    _levelLoader = new FileLevelLoader(_settings.levelStoreURI, _chunkStorageFabric);
                    _levelSaver = new FileLevelSaver(_settings.levelStoreURI, true, _chunkStorageFabric);
                    break;

                default:
                    throw new Exception($"Unknown chunk storage strategy {_settings.chunkStorageStrategy}");
            }

            _blockProtoCollection = new BlockProtoCollection();
            _gridSettingsCollection = new GridSettingsCollection();
            _gridStatesCollection = new GridStatesCollection(this);
            _userManager = new();
        }

        public void Destroy()
        {
            _gridStatesCollection.Destroy();
            _gridSettingsCollection.Destroy();
            _blockProtoCollection.Destroy();
        }

        #region Public API

        public GridSettingsCollection GridSettingsCollection => _gridSettingsCollection;
        public BlockProtoCollection BlockProtoCollection => _blockProtoCollection;
        public GridStatesCollection GridStatesCollection => _gridStatesCollection;
        public UserManager UserManager => _userManager;
        public LevelSettings LevelSettings => _settings;

        internal ChunkStorageFabric ChunkStorageFabric => _chunkStorageFabric;

        public void SaveLevel(string levelPath = null)
        {
            if (_settings.chunkStorageStrategy == ChunkStorageStrategy.DontSave) {
                throw new LevelAPIException($"Level saving not available");
            }
            _levelSaver.SaveFullContent(this, levelPath);
        }

        public void LoadLevel(string levelPath = null)
        {
            if (_settings.chunkStorageStrategy == ChunkStorageStrategy.DontSave) {
                throw new LevelAPIException($"Level loading not available");
            }
            _levelLoader.LoadFullContent(this, levelPath);
        }

        #endregion Public API
    }

    public static class LevelAPITools
    {
        public static void Clear(Transform target)
        {
            while (target.childCount > 0) {
                GameObject.DestroyImmediate(target.GetChild(0).gameObject);
            }
        }
    }
}