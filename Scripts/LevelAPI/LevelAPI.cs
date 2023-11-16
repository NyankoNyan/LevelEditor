using System;
using UnityEngine;

namespace Level.API
{
    public class LevelAPIException : Exception
    {
        public LevelAPIException(string msg) : base( msg )
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

        public LevelAPI()
        {
            _blockProtoCollection = new BlockProtoCollection();
            _gridSettingsCollection = new GridSettingsCollection();
            _gridStatesCollection = new GridStatesCollection( this );
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

        public void TODORefactorSaveLevel(Level.IO.ILevelSave levelSaver)
        {
            levelSaver.SaveFullContent( this );
        }

        public void TODORefactorLoadLevel(Level.IO.ILevelLoader levelLoader)
        {
            levelLoader.LoadFullContent( this );
        }

        #endregion Public API
    }

    public static class LevelAPITools
    {
        public static void Clear(Transform target)
        {
            while (target.childCount > 0) {
                GameObject.DestroyImmediate( target.GetChild( 0 ) );
            }
        }
    }

    public interface IObjectViewReceiver
    {
        Action removed { get; set; }
        Action<bool> visibilityChanged { get; set; }
        bool Visible { get; }

        void Remove();
    }

    /// <summary>
    /// Прослойка для доступа к единственному блоку.
    /// Объект блока как таковой в модели не существует.
    /// </summary>
    public class BlockViewAPI : IObjectViewReceiver
    {
        private BlockLayer<BlockData> _blockLayer;
        private int _flatCoord;

        public BlockViewAPI(BlockLayer<BlockData> blockLayer, Vector3Int blockCoord, GridSettings gridSettings)
        {
            _blockLayer = blockLayer ?? throw new ArgumentNullException( nameof( blockLayer ) );
            _flatCoord = GridState.BlockCoordToFlat( blockCoord, gridSettings.ChunkSize );

            Action<DataLayerEventArgs> changed = (args) => {
                if (args is BlockLevelLoadedEventArgs loadArgs) {
                    if (loadArgs.blockCoord == blockCoord) {
                        BlockData blockData = blockLayer.GetData( loadArgs.blockCoord);
                        if (blockData.blockId == 0) {
                            removed?.Invoke();
                        }
                    }
                }
            };
            blockLayer.changed += changed;

            Action<DataLayerSettings> layerRemoved = null;
            layerRemoved = (layerSettings) => {
                if (layerSettings.tag == _blockLayer.Tag) {
                    blockLayer.changed -= changed;
                    gridSettings.layerRemoved -= layerRemoved;

                    removed?.Invoke();
                }
            };
            gridSettings.layerRemoved += layerRemoved;
        }

        public Action removed { get; set; }

        public Action<bool> visibilityChanged
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool Visible => throw new NotImplementedException();

        public void Remove()
        {
            BlockData blockData = default;
            _blockLayer.SetItem( _flatCoord, blockData );
        }
    }
}