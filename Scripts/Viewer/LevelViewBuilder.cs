using Level;
using Level.API;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace LevelView
{
    /// <summary>
    /// Формирует вьюху по модели. Да, да, это тот самый класс вьюхи.
    /// </summary>
    public class LevelViewBuilder
    {
        private ConstructFabric _constructFabric;
        private LevelAPI _levelAPI;
        private IObjectViewFabric _objViewFabric;

        private Dictionary<DataLayer, BlockLayerSyncronizer> _blockSyncronizers = new();


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

            // Read grid settings
            _levelAPI.BlockProtoCollection.added = ReactiveTools.SubscribeCollection(
                _levelAPI.BlockProtoCollection,
                _levelAPI.BlockProtoCollection.added,
                (blockProto) => SetupBlockProto(blockProto)
            );
            _levelAPI.BlockProtoCollection.removed += RemoveBlockProto;

            // Read grid states
            _levelAPI.GridStatesCollection.added = ReactiveTools.SubscribeCollection(
                _levelAPI.GridStatesCollection,
                _levelAPI.GridStatesCollection.added,
                (gridState) => SetupGridState(gridState, root)
            );
        }


        private void SetupBlockProto(BlockProto blockProto)
        {
            // Простая проверка на существование префаба
            if (!_objViewFabric.HasPrefab(blockProto.Name)) {
                Debug.LogError($"Missing block class {blockProto.Name}");
            }
        }

        private void RemoveBlockProto(BlockProto proto)
        {
            // Удаление описания блока не имеет смысла для вьюхи, 
            // но мы можем попробовать удалить пул объектов 
            _objViewFabric.Unuse(proto.Name);
        }

        private void SetupGridState(GridState gridState, Transform parent)
        {
            // Корневой объект для хранения грида
            GameObject gridView = new($"{gridState.Key}-{gridState.GridSettingsName}");
            gridView.transform.parent = parent;
            gridView.transform.localPosition = default;
            gridView.transform.localRotation = Quaternion.identity;

            Action<GridState, DataLayer> onLayerAdded = (gridState, dataLayer) => {
                SetupDataLayer(dataLayer, parent, gridState);
            };
            gridState.layerAdded += onLayerAdded;
            foreach (var dataLayer in gridState.DataLayers) {
                SetupDataLayer(dataLayer, gridView.transform, gridState);
            }

            Action<GridState, string> onLayerRemoved = (gridState, layerTag) => {
                var dataLayer = gridState.GetLayer(layerTag);
                RemoveDataLayer(dataLayer, gridView.transform);
            };
            gridState.layerRemoved += onLayerRemoved;

            Action onDestroy = null;
            onDestroy = () => {
                gridState.layerAdded -= onLayerAdded;
                gridState.layerRemoved -= onLayerRemoved;
                gridState.OnDestroyAction -= onDestroy;
                RemoveGridState(gridState, gridView.transform);
            };
            gridState.OnDestroyAction += onDestroy;
        }

        private void RemoveGridState(GridState gridState, Transform gridView)
        {
            foreach (var dataLayer in gridState.DataLayers) {
                RemoveDataLayer(dataLayer, gridView.transform);
            }
            GameObject.Destroy(gridView.gameObject);
        }

        private void SetupDataLayer(DataLayer dataLayer, Transform gridView, GridState gridState)
        {
            var layerSettings = gridState.GridSettings.Settings.layers.Single(x => x.tag == dataLayer.Tag);
            switch (dataLayer.LayerType) {
                case LayerType.BlockLayer:
                    var layerSyncronizer = new BlockLayerSyncronizer(
                        _levelAPI,
                        gridState,
                        (ChunkLayer<BlockData, Vector3Int>)dataLayer,
                        _objViewFabric,
                        gridView);
                    layerSyncronizer.Init();
                    _blockSyncronizers.Add(dataLayer, layerSyncronizer);
                    //TODO add remove layer handler
                    break;

                default:
                    Debug.LogError($"Layer {dataLayer.LayerType} not supported");
                    break;
            }
        }

        private void RemoveDataLayer(DataLayer dataLayer, Transform gridView)
        {
            switch (dataLayer.LayerType) {
                case LayerType.BlockLayer:
                    var layerSyncronizer = _blockSyncronizers[dataLayer];
                    layerSyncronizer.Destroy();
                    _blockSyncronizers.Remove(dataLayer);
                    break;
                default:
                    Debug.LogError($"Layer {dataLayer.LayerType} not supported");
                    break;
            }
        }
    }
}
