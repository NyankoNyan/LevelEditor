using Level;
using Level.API;
using System;
using UnityEngine;

namespace LevelView
{
    public class LevelViewBuilder
    {
        private IConstructFabric _constructFabric;
        private LevelAPI _levelAPI;
        private IObjectViewFabric _objViewFabric;

        public LevelViewBuilder(IConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
        }

        public void Build(LevelAPI levelAPI, Transform root, bool ignorePools)
        {
            _levelAPI = levelAPI;
            if (ignorePools) {
                _objViewFabric = new ObjectViewFabricNonPool( _constructFabric );
            } else {
                _objViewFabric = new ObjectViewFabric( _constructFabric );
            }

            levelAPI.BlockProtoCollection.added += (blockProto) => {
                SetupBlockProto( blockProto, _constructFabric );
            };
            foreach (var blockProto in levelAPI.BlockProtoCollection) {
                SetupBlockProto( blockProto, _constructFabric );
            }

            // Setup grid states
            Action<GridState> onStateAdded = (gridState) => {
                SetupGridState( gridState, root );
            };
            levelAPI.GridStatesCollection.added += onStateAdded;
            foreach (var gridState in levelAPI.GridStatesCollection) {
                SetupGridState( gridState, root );
            }
        }

        private void SetupBlockProto(BlockProto blockProto, IConstructFabric constructFabric)
        {
            if (!constructFabric.HasPrefab( blockProto.Name )) {
                Debug.LogError( $"Missing block prefab {blockProto.Name}" );
            }
        }

        private void SetupGridState(GridState gridState, Transform parent)
        {
            GameObject gridView = new( $"{gridState.Key}-{gridState.GridSettingsName}" );
            gridView.transform.parent = parent;
            gridView.transform.localPosition = default;
            gridView.transform.localRotation = Quaternion.identity;

            Action<DataLayerEventArgs, GridState> layerChanged = (args, gridState) => {
                if(args is BlockLevelLoadedEventArgs loadArgs) {
                    SetupDataLayer( loadArgs.dataLayer, parent, gridState.GridSettings );
                }
            };
            gridState.layerChanged += layerChanged;

            foreach (var dataLayer in gridState.DataLayers) {
                SetupDataLayer( dataLayer, parent, gridState.GridSettings );
            }

            Action onDestroy = null;
            onDestroy = () => {
                gridState.layerChanged -= layerChanged;
                gridState.OnDestroyAction -= onDestroy;
            };
            gridState.OnDestroyAction += onDestroy;
        }

        private void SetupDataLayer(DataLayer dataLayer, Transform parent, GridSettings gridSettings)
        {
            switch (dataLayer.LayerType) {
                case LayerType.BlockLayer:
                    SetupBlockLayer( dataLayer as BlockLayer<BlockData>, parent, gridSettings );
                    break;

                default:
                    Debug.LogError( $"Layer {dataLayer.LayerType} not supported" );
                    break;
            }
        }

        private void SetupBlockLayer(
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            foreach(var chunkCoord in blockLayer.LoadedChunks) {
                GameObject layerGO = new( "Block layer");
                layerGO.transform.parent = parent;
                layerGO.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
                DataLayerContent<BlockData> chunkData = blockLayer.GetChunkData( chunkCoord );                
                SetupBlockChunk( chunkCoord, chunkData, layerGO.transform, gridSettings );
            }

            //for (int i = 0; i < gridSettings.ChunkSizeFlat; i++) {
            //    var blockData = blockLayer.Item( i );
            //    if (blockData.blockId == 0) {
            //        continue;
            //    }
            //    Vector3Int blockCoord = GridChunk.FlatToBlockCoord( i, gridSettings.ChunkSize );
            //    Vector3 pos = new Vector3(
            //        blockCoord.x * gridSettings.CellSize.x,
            //        blockCoord.y * gridSettings.CellSize.y,
            //        blockCoord.z * gridSettings.CellSize.z );
            //    BlockProto blockProto = _levelAPI.BlockProtoCollection[blockData.blockId];
            //    BlockViewAPI blockViewAPI = new BlockViewAPI( blockLayer, blockCoord, gridSettings );
            //    var objectView = _objViewFabric.Create( blockProto.Name, blockViewAPI );
            //    objectView.transform.parent = parent;
            //    objectView.transform.localRotation = Quaternion.identity;
            //    objectView.transform.localPosition = pos;
            //}
        }

        private void SetupBlockChunk(
            Vector3Int chunkCoord,
            DataLayerContent<BlockData> chunkData,
            Transform parent,
            GridSettings gridSettings)
        {
            GameObject chunkView = new( $"{chunkCoord.x} {chunkCoord.y} {chunkCoord.z}" );
            chunkView.transform.parent = parent;
            chunkView.transform.localRotation = Quaternion.identity;
            chunkView.transform.localPosition = new Vector3(
                gridSettings.ChunkSize.x * chunkCoord.x,
                gridSettings.ChunkSize.y * chunkCoord.y,
                gridSettings.ChunkSize.z * chunkCoord.z
                );

            for (int i = 0; i < gridSettings.ChunkSizeFlat; i++) {
                var blockData = chunkData[ (uint)i ];
                if (blockData.blockId == 0) {
                    continue;
                }
                Vector3Int blockCoord = GridState.FlatToBlockCoord( i, gridSettings.ChunkSize );
                Vector3 pos = new Vector3(
                    blockCoord.x * gridSettings.CellSize.x,
                    blockCoord.y * gridSettings.CellSize.y,
                    blockCoord.z * gridSettings.CellSize.z );
                BlockProto blockProto = _levelAPI.BlockProtoCollection[blockData.blockId];
                BlockViewAPI blockViewAPI = new BlockViewAPI( blockLayer, blockCoord, gridSettings );
                var objectView = _objViewFabric.Create( blockProto.Name, blockViewAPI );
                objectView.transform.parent = parent;
                objectView.transform.localRotation = Quaternion.identity;
                objectView.transform.localPosition = pos;
            }

            //Action<DataLayer> layerAdded = (dataLayer) => {
            //    SetupLayer( dataLayer, chunkView.transform, gridSettings );
            //};
            //chunk.layerAdded += layerAdded;

            //foreach (var dataLayer in chunk.Layers) {
            //    SetupLayer( dataLayer, chunkView.transform, gridSettings );
            //}

            //Action onRemove = null;
            //onRemove = () => {
            //    chunk.layerAdded -= layerAdded;
            //    chunk.OnDestroyAction -= onRemove;
            //};
            //chunk.OnDestroyAction += onRemove;
        }

        private void SetupLayer(
            DataLayer dataLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            switch (dataLayer.LayerType) {
                case LayerType.BlockLayer:
                    SetupBlockLayer( dataLayer as BlockLayer, parent, gridSettings );
                    break;

                default:
                    Debug.LogError( $"Layer {dataLayer.LayerType} not supported" );
                    break;
            }
        }

        
    }
}