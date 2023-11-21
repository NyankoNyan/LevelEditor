using Level;
using Level.API;

using System;
using System.Collections;

using UnityEngine;

namespace LevelView
{
    public class LevelViewBuilder
    {
        private ConstructFabric _constructFabric;
        private LevelAPI _levelAPI;
        private IObjectViewFabric _objViewFabric;

        public LevelViewBuilder(ConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
        }

        public void Build(LevelAPI levelAPI, Transform root, bool ignorePools)
        {
            _levelAPI = levelAPI;
            if (ignorePools) {
                _objViewFabric = new ObjectViewFabricNonPool(_constructFabric);
            } else {
                _objViewFabric = new ObjectViewFabric(_constructFabric);
            }

            // Setup grid settings
            ReactiveTools.SubscribeCollection(
                _levelAPI.BlockProtoCollection,
                _levelAPI.BlockProtoCollection.added,
                (blockProto) => SetupBlockProto(blockProto, _constructFabric)
            );

            // Setup grid states
            ReactiveTools.SubscribeCollection(
                _levelAPI.GridStatesCollection,
                _levelAPI.GridStatesCollection.added,
                (gridState) => SetupGridState(gridState, root)
            );
        }

        private void SetupBlockProto(BlockProto blockProto, ConstructFabric constructFabric)
        {
            // Простая проверка на существование префаба
            if (!constructFabric.HasRefId(blockProto.Name)) {
                Debug.LogError($"Missing block class {blockProto.Name}");
            }
        }

        private void SetupGridState(GridState gridState, Transform parent)
        {
            GameObject gridView = new($"{gridState.Key}-{gridState.GridSettingsName}");
            gridView.transform.parent = parent;
            gridView.transform.localPosition = default;
            gridView.transform.localRotation = Quaternion.identity;

            Action<DataLayerEventArgs, GridState> layerChanged = (args, gridState) => {
                if (args is BlockLevelLoadedEventArgs loadArgs) {
                    SetupDataLayer(loadArgs.dataLayer, parent, gridState.GridSettings);
                }
            };
            gridState.layerChanged += layerChanged;

            foreach (var dataLayer in gridState.DataLayers) {
                SetupDataLayer(dataLayer, parent, gridState.GridSettings);
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
                    SetupBlockLayer(dataLayer as BlockLayer<BlockData>, parent, gridSettings);
                    break;

                default:
                    Debug.LogError($"Layer {dataLayer.LayerType} not supported");
                    break;
            }
        }

        private void SetupBlockLayer(
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            foreach (var chunkCoord in blockLayer.LoadedChunks) {
                GameObject layerGO = new("Block layer");
                layerGO.transform.parent = parent;
                layerGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                DataLayerContent<BlockData> chunkData = blockLayer.GetChunkData(chunkCoord);
                SetupBlockChunk(chunkCoord, chunkData, blockLayer, layerGO.transform, gridSettings);
            }
        }

        private void SetupBlockChunk(
            Vector3Int chunkCoord,
            DataLayerContent<BlockData> chunkData,
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            GameObject chunkView = new($"{chunkCoord.x} {chunkCoord.y} {chunkCoord.z}");
            chunkView.transform.parent = parent;
            chunkView.transform.localRotation = Quaternion.identity;
            chunkView.transform.localPosition = new Vector3(
                gridSettings.ChunkSize.x * chunkCoord.x,
                gridSettings.ChunkSize.y * chunkCoord.y,
                gridSettings.ChunkSize.z * chunkCoord.z
                );

            for (int i = 0; i < gridSettings.ChunkSizeFlat; i++) {
                var blockData = chunkData[(uint)i];
                if (blockData.blockId == 0) {
                    continue;
                }
                Vector3Int blockCoord = GridState.FlatToBlockCoord(i, gridSettings.ChunkSize);
                Vector3 pos = new Vector3(
                    blockCoord.x * gridSettings.CellSize.x,
                    blockCoord.y * gridSettings.CellSize.y,
                    blockCoord.z * gridSettings.CellSize.z);
                BlockProto blockProto = _levelAPI.BlockProtoCollection[blockData.blockId];
                BlockViewAPI blockViewAPI = new BlockViewAPI(blockLayer, blockCoord, gridSettings);
                var objectView = _objViewFabric.Create(blockProto.Name, blockViewAPI);
                objectView.transform.parent = parent;
                objectView.transform.localRotation = Quaternion.identity;
                objectView.transform.localPosition = pos;
            }
        }

        private void SetupLayer(
            DataLayer dataLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            switch (dataLayer.LayerType) {
                case LayerType.BlockLayer:
                    SetupBlockLayer(dataLayer as BlockLayer<BlockData>, parent, gridSettings);
                    break;

                default:
                    Debug.LogError($"Layer {dataLayer.LayerType} not supported");
                    break;
            }
        }
    }
}

public static class ReactiveTools
{
    public static Action<TValue1> SubscribeCollection<TValue1>(
        IEnumerable<TValue1> collection,
        Action<TValue1> action,
        Action<TValue1> handler)
    {
        action += handler;
        foreach (TValue1 value in collection) {
            handler(value);
        }
        return action;
    }

    public static Action<TValue1, TValue2> SubscribeCollection<TValue1,TValue2>(
        IEnumerable<TValue1> collection,
        Action<TValue1, TValue2> action,
        TValue2 value2,        
        Action<TValue1, TValue2> handler)
    {
        action += handler;
        foreach (TValue1 value in collection) {
            handler(value, value2);
        }
        return action;
    }
}