using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Level.API
{
    public class LevelAPIException : Exception
    {
        public LevelAPIException(string msg) : base( msg ) { }
    }


    public class LevelAPIFabric
    {
        public LevelAPI Create()
        {
            var gridSettingsEnv = new TypeEnv<GridSettings, uint, GridSettingsCore, GridSettingsFabric, GridSettingsRegistry>();
            var blockProtoEnv = new TypeEnv<BlockProto, uint, BlockProtoCreateParams, BlockProtoFabric, BlockProtoRegistry>();
            var gridStateEnv = new TypeEnv<GridState, uint, GridStateCreateParams, GridStateFabric, GridStateRegistry>();
            var dataLayerFabric = new DataLayerFabric();

            return new LevelAPI(
                gridSettingsEnv.Registry,
                gridSettingsEnv.Fabric,
                blockProtoEnv.Registry,
                blockProtoEnv.Fabric,
                gridStateEnv.Registry,
                gridStateEnv.Fabric,
                dataLayerFabric
                );
        }
    }

    /// <summary>
    /// Предоставляет доступ к API уровня в целом и отдельным его компонентам.
    /// </summary>
    public class LevelAPI
    {
        private GridSettingsAPI _gridSettingsAPI;
        private BlockProtoAPI _blockProtoAPI;
        //private DataLayerAPIFabric _dataLayerAPIFabric;
        private GridStatesAPI _gridStateAPI;
        private BlockProtoRegistry TODORefactor_blockProtoRegistry;
        private GridSettingsRegistry TODORefactor_gridSettingsRegistry;
        private GridStateRegistry TODORefactor_gridStateRegistry;

        private GridSettingsCollection _gridSettingsCollection;

        public LevelAPI(
            GridSettingsRegistry gridSettingsRegistry,
            GridSettingsFabric gridSettingsFabric,
            BlockProtoRegistry blockProtoRegistry,
            BlockProtoFabric blockProtoFabric,
            GridStateRegistry gridStateRegistry,
            GridStateFabric gridStateFabric,
            DataLayerFabric dataLayerFabric
            )
        {
            _gridSettingsAPI = new GridSettingsAPI( gridSettingsRegistry, gridSettingsFabric );
            _blockProtoAPI = new BlockProtoAPI( blockProtoRegistry, blockProtoFabric );
            //_dataLayerAPIFabric = new DataLayerAPIFabric();
            _gridStateAPI = new GridStatesAPI( gridStateRegistry, gridStateFabric, _gridSettingsAPI, dataLayerFabric );

            TODORefactor_blockProtoRegistry = blockProtoRegistry;
            TODORefactor_gridSettingsRegistry = gridSettingsRegistry;
            TODORefactor_gridStateRegistry = gridStateRegistry;

            _gridSettingsCollection = new();
        }

        #region Public API

        public GridSettingsCollection => _gridSettingsCollection;

        public IGridSettingsAPI GridSettings => _gridSettingsAPI;
        public IBlockProtoAPI BlockProto => _blockProtoAPI;
        public IGridStatesAPI GridStates => _gridStateAPI;

        void ILevelAPI.TODORefactorSaveLevel(Level.IO.ILevelSave levelSaver)
        {
            levelSaver.SaveFullContent(
                TODORefactor_blockProtoRegistry,
                TODORefactor_gridSettingsRegistry,
                TODORefactor_gridStateRegistry );
        }

        void ILevelAPI.TODORefactorLoadLevel(Level.IO.ILevelLoader levelLoader)
        {
            levelLoader.LoadFullContent( this );
        }

        #endregion
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
        UnityAction removed { get; set; }
        UnityAction<bool> visibilityChanged { get; set; }
        bool Visible { get; }
        void Remove();
    }

    /// <summary>
    /// Прослойка для доступа к единственному блоку.
    /// Объект блока как таковой в модели не существует.
    /// </summary>
    public class BlockViewAPI : IObjectViewReceiver
    {
        private BlockLayer _blockLayer;
        private int _flatCoord;

        public BlockViewAPI(BlockLayer blockLayer, Vector3Int blockCoord, GridSettings gridSettings)
        {
            _blockLayer = blockLayer ?? throw new ArgumentNullException( nameof( blockLayer ) );
            _flatCoord = GridChunk.BlockCoordToFlat( blockCoord, gridSettings.ChunkSize );

            UnityAction<int> onChanged = (i) => {
                if (i == _flatCoord) {
                    BlockData blockData = blockLayer.Item( i );
                    if (blockData.blockId == 0) {
                        removed?.Invoke();
                    }
                }
            };
            blockLayer.onChanged += onChanged;

            UnityAction<DataLayerSettings> layerRemoved = null;
            layerRemoved = (layerSettings) => {
                if (layerSettings.tag == _blockLayer.Tag) {
                    blockLayer.onChanged -= onChanged;
                    gridSettings.layerRemoved -= layerRemoved;

                    removed?.Invoke();
                }
            };
            gridSettings.layerRemoved += layerRemoved;
        }

        public UnityAction removed { get; set; }
        public UnityAction<bool> visibilityChanged
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
