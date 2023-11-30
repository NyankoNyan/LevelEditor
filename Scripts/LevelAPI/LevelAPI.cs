using System;

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
        public LevelAPI Create()
        {
            return new LevelAPI();
        }
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

        public LevelAPI(ChunkStorageFabric chunkStorageFabric = null)
        {
            if (chunkStorageFabric != null) {
                _chunkStorageFabric = chunkStorageFabric;
            } else {
                _chunkStorageFabric = new MockChunkStorageFabric();
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

        internal ChunkStorageFabric ChunkStorageFabric => _chunkStorageFabric;

        public void TODORefactorSaveLevel(Level.IO.ILevelSave levelSaver)
        {
            levelSaver.SaveFullContent(this);
        }

        public void TODORefactorLoadLevel(Level.IO.ILevelLoader levelLoader)
        {
            levelLoader.LoadFullContent(this);
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