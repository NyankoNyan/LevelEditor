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
            //TODO onRemoved

            // Setup grid states
            ReactiveTools.SubscribeCollection(
                _levelAPI.GridStatesCollection,
                _levelAPI.GridStatesCollection.added,
                (gridState) => SetupGridState(gridState, root)
            );
            //TODO onRemoved
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
            // Корневой объект для хранения грида
            GameObject gridView = new($"{gridState.Key}-{gridState.GridSettingsName}");
            gridView.transform.parent = parent;
            gridView.transform.localPosition = default;
            gridView.transform.localRotation = Quaternion.identity;

            Action<GridState, DataLayer> onLayerAdded = (gridState, dataLayer)=>{
                SetupDataLayer(dataLayer, parent, gridState.GridSettings);
            };
            gridState.layerAdded += onLayerAdded;
            foreach (var dataLayer in gridState.DataLayers) {
                SetupDataLayer(dataLayer, parent, gridState.GridSettings);
            }

            Action<GridState, string> onLayerRemoved = (gridState, layerTag)=>{
                //TODO something
            };
            gridState.layerRemoved += onLayerRemoved;

            Action onDestroy = null;
            onDestroy = () => {
                gridState.layerAdded -= onLayerAdded;
                gridState.layerRemoved -= onLayerRemoved;
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
            GridSettings gridSettings,
            DataLayerSettings dataLayerSettings)
        {
            GameObject layerView = new(dataLayerSettings.tag);
            layerView.transform.parent = parent;
            layerView.transform.localPosition = default;
            layerView.transform.localRotation = Quaternion.identity;

            blockLayer.chunkAdded += (chunkCoord)=>{
                SetupBlockChunk(chunkCoord, blockLayer, layerView, gridSettings);
            };
            foreach (var chunkCoord in blockLayer.LoadedChunks) {
                 SetupBlockChunk(chunkCoord, blockLayer, layerView, gridSettings);
            }

            blockLayer.chunkRemoved += (chunkCoord)=>{
                //TODO something
            };
        }

        private void SetupBlockChunk(
            Vector3Int chunkCoord,
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            GameObject chunkView = new($"{chunkCoord.x}-{chunkCoord.y}-{chunkCoord.z}");
            chunkView.transform.parent = parent;
            chunkView.transform.localRotation = Quaternion.identity;
            chunkView.transform.localPosition = new Vector3(
                gridSettings.ChunkSize.x * chunkCoord.x,
                gridSettings.ChunkSize.y * chunkCoord.y,
                gridSettings.ChunkSize.z * chunkCoord.z
                );

            var content = (DataLayerStaticContent<BlockData>) blockLayer.GetChunkData(chunkCoord);

            for(int i=0;i<content.Size;i++){
                BlockData data = content[i];
                // Block exists
                if(data.blockId!=0){
                    Vector3Int localBlockCoord = GridState.FlatToBlockCoord(i, blockLayer.ChunkSize);
                    Vector3 pos = new Vector3(
                        localBlockCoord.x * gridSettings.CellSize.x,
                        localBlockCoord.y * gridSettings.CellSize.y,
                        localBlockCoord.z * gridSettings.CellSize.z);
                    BlockProto blockProto = _levelAPI.BlockProtoCollection[data.blockId];
                    _objViewFabric.Create(blockProto.Name);
                    objectView.transform.parent = parent;
                    objectView.transform.localRotation = BlockData.DecodeRotation(data.rotation);
                    objectView.transform.localPosition = pos;
                }
            }            
        }

        private void RemoveBlockChunk()
        {
            //TODO remove
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