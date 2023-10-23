﻿using Level;
using Level.API;
using UnityEngine;
using UnityEngine.Events;

namespace LevelView
{
    public class LevelViewBuilder
    {
        private IConstructFabric _constructFabric;
        private ILevelAPI _levelAPI;
        private IObjectViewFabric _objViewFabric;

        public LevelViewBuilder(IConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
        }

        public void Build(ILevelAPI levelAPI, Transform root, bool ignorePools)
        {
            _levelAPI = levelAPI;
            if (ignorePools) {
                _objViewFabric = new ObjectViewFabricNonPool( _constructFabric );
            } else {
                _objViewFabric = new ObjectViewFabric( _constructFabric );
            }

            levelAPI.BlockProto.onBlockProtoAdded += (blockProto) => {
                SetupBlockProto( blockProto, _constructFabric );
            };
            foreach (var blockProto in levelAPI.BlockProto.BlockProtos) {
                SetupBlockProto( blockProto, _constructFabric );
            }

            // Setup grid states
            UnityAction<GridState> onStateAdded = (gridState) => {
                SetupGridState( gridState, root );
            };
            levelAPI.GridStates.onStateAdded += onStateAdded;
            foreach (var gridState in levelAPI.GridStates.Grids) {
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

            UnityAction<GridChunk> chunkLoaded = (chunk) => {
                SetupChunk( chunk, gridView.transform, gridState.GridSettings );
            };
            gridState.chunkLoaded += chunkLoaded;

            foreach (var chunk in gridState.LoadedChunks) {
                SetupChunk( chunk, gridView.transform, gridState.GridSettings );
            }

            UnityAction onDestroy = null;
            onDestroy = () => {
                gridState.chunkLoaded -= chunkLoaded;
                gridState.OnDestroyAction -= onDestroy;
            };
            gridState.OnDestroyAction += onDestroy;
        }

        private void SetupChunk(
            GridChunk chunk,
            Transform parent,
            GridSettings gridSettings)
        {
            GameObject chunkView = new( $"{chunk.Key.x} {chunk.Key.y} {chunk.Key.z}" );
            chunkView.transform.parent = parent;
            chunkView.transform.localRotation = Quaternion.identity;
            chunkView.transform.localPosition = new Vector3(
                gridSettings.ChunkSize.x * chunk.Key.x,
                gridSettings.ChunkSize.y * chunk.Key.y,
                gridSettings.ChunkSize.z * chunk.Key.z
                );

            UnityAction<DataLayer> layerAdded = (dataLayer) => {
                SetupLayer( dataLayer, chunkView.transform, gridSettings );
            };
            chunk.layerAdded += layerAdded;

            foreach (var dataLayer in chunk.Layers) {
                SetupLayer( dataLayer, chunkView.transform, gridSettings );
            }

            UnityAction onRemove = null;
            onRemove = () => {
                chunk.layerAdded -= layerAdded;
                chunk.OnDestroyAction -= onRemove;
            };
            chunk.OnDestroyAction += onRemove;
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

        private void SetupBlockLayer(
            BlockLayer blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            for (int i = 0; i < gridSettings.ChunkSizeFlat; i++) {
                var blockData = blockLayer.Item( i );
                if (blockData.blockId == 0) {
                    continue;
                }
                Vector3Int blockCoord = GridChunk.FlatToBlockCoord( i, gridSettings.ChunkSize );
                Vector3 pos = new Vector3(
                    blockCoord.x * gridSettings.CellSize.x,
                    blockCoord.y * gridSettings.CellSize.y,
                    blockCoord.z * gridSettings.CellSize.z );
                BlockProto blockProto = _levelAPI.BlockProto.GetBlockProto( blockData.blockId );
                BlockViewAPI blockViewAPI = new BlockViewAPI( blockLayer, blockCoord, gridSettings );
                var objectView = _objViewFabric.Create( blockProto.Name, blockViewAPI );
                objectView.transform.parent = parent;
                objectView.transform.localRotation = Quaternion.identity;
                objectView.transform.localPosition = pos;
            }
        }
    }
}
