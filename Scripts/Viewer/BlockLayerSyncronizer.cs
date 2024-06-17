using System;
using System.Collections.Generic;
using System.Linq;

using Level;
using Level.API;

using UnityEngine;

namespace LevelView
{
    /// <summary>
    /// Объект цепляется сверху слоя блоков, чтобы синхронизиовать с ним вьюху
    /// </summary>
    public class BlockLayerSyncronizer : ViewChunkLayerSyncronizer<BlockData, Vector3Int>
    {
        private Transform _layerView;
        private readonly Dictionary<Vector3Int, GameObject> _viewsGlobalIndex = new();
        private readonly Dictionary<GameObject, Vector3Int> _viewsGlobalReverseIndex = new();

        public BlockLayerSyncronizer(
            LevelAPI level,
            GridState gridState,
            ChunkLayer<BlockData, Vector3Int> chunkLayer,
            IObjectViewFabric objViewFabric,
            Transform gridTransform)
            : base(level, gridState, chunkLayer, objViewFabric, gridTransform)
        {
        }

        protected override void OnInit()
        {
            _viewDataLayer = (BigBlockLayer<ClientViewData>)_gridState.AddViewLayer(_chunkLayer.Tag + "_VIEW", _chunkLayer);
            _chunkLayer.chunkAdded += OnChunkAdded;
            _chunkLayer.chunkRemoved += OnChunkRemoved;
            _chunkLayer.changed += OnLayerChanged;
            _layerView = new GameObject(_chunkLayer.Tag).transform;
            _layerView.parent = _gridTransform;
            _layerView.localPosition = Vector3.zero;
            _layerView.localRotation = Quaternion.identity;

            foreach (var chunkCoord in _chunkLayer.LoadedChunks) {
                SetupChunkView(chunkCoord);
            }
        }

        protected override void OnDestroy()
        {
            _gridState.RemoveViewLayer(_viewDataLayer.Tag);
            _chunkLayer.chunkAdded -= OnChunkAdded;
            _chunkLayer.chunkRemoved -= OnChunkRemoved;
            _chunkLayer.changed -= OnLayerChanged;

            foreach (var blockCoord in _viewsGlobalIndex.Keys.ToArray()) {
                RemoveBlock(blockCoord);
            }

            GameObject.Destroy(_layerView.gameObject);
        }

        private void OnLayerChanged(DataLayerEventArgs args)
        {
            //if (args is BlockLayerChangedEventArgs changeArgs) {
            //    foreach (var info in changeArgs.changed) {
            //        var blockLayer = (BlockLayer<BlockData>)args.dataLayer;
            //        Vector3Int globalBlockCoord = blockLayer.BlockGlobalCoord(info.dataKey.chunkCoord, info.dataKey.dataId);
            //        ChangeBlockTo(globalBlockCoord, info.blockData);
            //    }
            //}else 
            if(args is ChunkLayerChangedEventArgs<BlockData> chunkArgs) {
                foreach(var info in chunkArgs.changed) {
                    var blockLayer = (BlockLayer<BlockData>)args.dataLayer;
                    Vector3Int globalBlockCoord = blockLayer.BlockGlobalCoord(info.key.chunkCoord, info.key.dataId);
                    ChangeBlockTo(globalBlockCoord, info.data);
                }
            }
        }

        private void OnChunkRemoved(Vector3Int chunkCoord)
        {
            RemoveChunkView(chunkCoord);
        }

        private void OnChunkAdded(Vector3Int chunkCoord)
        {
            SetupChunkView(chunkCoord);
        }

        private void SetupChunkView(Vector3Int chunkCoord)
        {
            BlockLayer<BlockData> blockLayer = (BlockLayer<BlockData>)_chunkLayer;
            Transform chunkView = new GameObject(GetChunkName(chunkCoord)).transform;
            Vector3 chunkSize = Vector3.Scale(_gridState.GridSettings.ChunkSize, _gridState.GridSettings.CellSize);

            chunkView.parent = _layerView;
            chunkView.localRotation = Quaternion.identity;
            chunkView.localPosition = Vector3.Scale(chunkSize, chunkCoord);

            var content = (DataLayerStaticContent<BlockData>)blockLayer.GetChunkData(chunkCoord);

            for (int i = 0; i < content.Size; i++) {
                BlockData data = content[i];
                if (data.blockId != 0) {
                    AddBlock(blockLayer.BlockGlobalCoord(chunkCoord, i), data);
                }
            }
        }

        private void RemoveChunkView(Vector3Int chunkCoord)
        {
            var chunkTransform = _layerView.Find(GetChunkName(chunkCoord));
            if (!chunkTransform) {
                throw new LevelAPIException($"Missing chunk {chunkCoord}");
            }
            // Все дочерние объекты должны утилизироваться с помощью специального метода
            for (int i = 0; i < chunkTransform.childCount; i++) {
                var subObj = chunkTransform.GetChild(i).gameObject;
                Vector3Int blockCoord = _viewsGlobalReverseIndex[subObj];
                RemoveBlock(blockCoord);
            }
            GameObject.Destroy(chunkTransform.gameObject);
        }

        private void AddBlock(
            Vector3Int globalBlockCoord,
            BlockData blockData)
        {
            BlockLayer<BlockData> blockLayer = (BlockLayer<BlockData>)_chunkLayer;
            var localBlockCoord = blockLayer.LocalCoordOfGlobalBlock(globalBlockCoord);
            var chunkCoord = blockLayer.GetChunkOfGlobalBlock(globalBlockCoord);

            var chunkRoot = _layerView.Find(GetChunkName(chunkCoord));
            if (!chunkRoot) {
                throw new Exception($"Not found chunk root {chunkCoord}");
            }

            if (blockData.blockId == 0) {
                throw new LevelAPIException($"Zero block id {_layerView} {localBlockCoord}");
            }

            Vector3 pos = Vector3.Scale(localBlockCoord, _gridState.GridSettings.CellSize);
            BlockProto blockProto = _level.BlockProtoCollection[blockData.blockId];
            var objectView = _objViewFabric.Create(blockProto.Name);
            objectView.transform.parent = chunkRoot;
            objectView.transform.localRotation = blockData.rotation.ToDiscreteAngle().ToQuaternion();
            objectView.transform.localPosition = pos;

            _viewsGlobalIndex.Add(globalBlockCoord, objectView);
            _viewsGlobalReverseIndex.Add(objectView, globalBlockCoord);
        }

        private void RemoveBlock(Vector3Int globalBlockCoord)
        {
            if (_viewsGlobalIndex.TryGetValue(globalBlockCoord, out GameObject blockView)) {
                _objViewFabric.Remove(blockView);
                _viewsGlobalIndex.Remove(globalBlockCoord);
                _viewsGlobalReverseIndex.Remove(blockView);
            } else {
                throw new LevelAPIException($"Missing block view with coord {globalBlockCoord}");
            }
        }

        private void ChangeBlockTo(Vector3Int globalBlockCoord, BlockData blockData)
        {
            try {
                RemoveBlock(globalBlockCoord);
            } catch { }

            if (blockData.blockId != 0) {
                AddBlock(globalBlockCoord, blockData);
            }
        }

        private static string GetChunkName(Vector3Int chunkCoord) => $"{chunkCoord.x}-{chunkCoord.y}-{chunkCoord.z}";
    }
}