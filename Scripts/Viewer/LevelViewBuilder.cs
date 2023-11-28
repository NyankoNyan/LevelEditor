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
        private Dictionary<Vector3Int, GameObjectInfo> _blocksGlobalNavi = new();

        public LevelViewBuilder(ConstructFabric constructFabric)
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

            // Setup grid settings
            ReactiveTools.SubscribeCollection(
                _levelAPI.BlockProtoCollection,
                _levelAPI.BlockProtoCollection.added,
                (blockProto) => SetupBlockProto( blockProto, _constructFabric )
            );
            //TODO onRemoved

            // Setup grid states
            ReactiveTools.SubscribeCollection(
                _levelAPI.GridStatesCollection,
                _levelAPI.GridStatesCollection.added,
                (gridState) => SetupGridState( gridState, root )
            );
            //TODO onRemoved
        }

        private void SetupBlockProto(BlockProto blockProto, ConstructFabric constructFabric)
        {
            // Простая проверка на существование префаба
            if (!constructFabric.HasRefId( blockProto.Name )) {
                Debug.LogError( $"Missing block class {blockProto.Name}" );
            }
        }

        private void SetupGridState(GridState gridState, Transform parent)
        {
            // Корневой объект для хранения грида
            GameObject gridView = new( $"{gridState.Key}-{gridState.GridSettingsName}" );
            gridView.transform.parent = parent;
            gridView.transform.localPosition = default;
            gridView.transform.localRotation = Quaternion.identity;

            Action<GridState, DataLayer> onLayerAdded = (gridState, dataLayer) => {
                SetupDataLayer( dataLayer, parent, gridState.GridSettings );
            };
            gridState.layerAdded += onLayerAdded;
            foreach (var dataLayer in gridState.DataLayers) {
                SetupDataLayer( dataLayer, parent, gridState.GridSettings );
            }

            Action<GridState, string> onLayerRemoved = (gridState, layerTag) => {
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
            var layerSettings = gridSettings.Settings.layers.Single( x => x.tag == dataLayer.Tag );
            switch (dataLayer.LayerType) {
                case LayerType.BlockLayer:
                    SetupBlockLayer( dataLayer as BlockLayer<BlockData>, parent, gridSettings, layerSettings );
                    break;

                default:
                    Debug.LogError( $"Layer {dataLayer.LayerType} not supported" );
                    break;
            }
        }

        private void SetupBlockLayer(
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings,
            DataLayerSettings dataLayerSettings)
        {
            GameObject layerView = new( dataLayerSettings.tag );
            layerView.transform.parent = parent;
            layerView.transform.localPosition = default;
            layerView.transform.localRotation = Quaternion.identity;

            blockLayer.chunkAdded += (chunkCoord) => {
                SetupBlockChunk( chunkCoord, blockLayer, layerView.transform, gridSettings );
            };
            foreach (var chunkCoord in blockLayer.LoadedChunks) {
                SetupBlockChunk( chunkCoord, blockLayer, layerView.transform, gridSettings );
            }

            blockLayer.changed += (args) => {
                if (args is BlockLayerChangedEventArgs blockArgs) {
                    foreach (var blockInfo in blockArgs.added) {
                        AddBlock(
                            blockInfo.globalCoord,
                            blockLayer,
                            blockInfo.blockData,
                            layerView.transform,
                            gridSettings.CellSize );
                    }
                    foreach (var blockInfo in blockArgs.changed) {
                    }
                    foreach (var blockCoord in blockArgs.removed) {
                    }
                }
            };

            blockLayer.chunkRemoved += (chunkCoord) => {
                RemoveBlockChunk( chunkCoord, layerView.transform );
            };
        }

        private void AddBlock(
            Vector3Int globalBlockCoord,
            BlockLayer<BlockData> blockLayer,
            BlockData blockData,
            Transform layerRoot,
            Vector3 cellSize)
        {
            var localBlockCoord = blockLayer.LocalCoordOfGlobalBlock( globalBlockCoord );
            var chunkCoord = blockLayer.GetChunkOfGlobalBlock( globalBlockCoord );

            var chunkRoot = layerRoot.Find( GetChunkName( chunkCoord ) );
            if (!chunkRoot) {
                throw new Exception( $"Not found chunk root {chunkCoord}" );
            }

            // AddBlock(localBlockCoord, gridSettings.CellSize, blockData, layerRoot);

            if (blockData.blockId == 0) {
                throw new LevelAPIException( $"Zero block id {layerRoot} {localBlockCoord}" );
            }

            Vector3 pos = new Vector3(
                localBlockCoord.x * cellSize.x,
                localBlockCoord.y * cellSize.y,
                localBlockCoord.z * cellSize.z );
            BlockProto blockProto = _levelAPI.BlockProtoCollection[blockData.blockId];
            var objectView = _objViewFabric.Create( blockProto.Name );
            objectView.transform.parent = layerRoot;
            objectView.transform.localRotation = BlockData.DecodeRotation( blockData.rotation );
            objectView.transform.localPosition = pos;

            GameObjectInfo goInfo = new() { };
            _blocksGlobalNavi.Add( globalBlockCoord, goInfo );
        }

        private void ChangeBlock()
        {
        }

        private void RemoveBlock(Vector3Int globalBlockCoord)
        {
        }

        private void RemoveBlock(GameObject gameObject)
        {
        }

        private void SetupBlockChunk(
            Vector3Int chunkCoord,
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            GameObject chunkView = new( GetChunkName( chunkCoord ) );
            chunkView.transform.parent = parent;
            chunkView.transform.localRotation = Quaternion.identity;
            chunkView.transform.localPosition = new Vector3(
                gridSettings.ChunkSize.x * chunkCoord.x,
                gridSettings.ChunkSize.y * chunkCoord.y,
                gridSettings.ChunkSize.z * chunkCoord.z
                );

            var content = (DataLayerStaticContent<BlockData>)blockLayer.GetChunkData( chunkCoord );

            for (int i = 0; i < content.Size; i++) {
                BlockData data = content[i];
                Vector3Int localBlockCoord = GridState.FlatToBlockCoord( i, blockLayer.ChunkSize );

                AddBlock( blockLayer.BlockGlobalCoord( chunkCoord, i ),
                         blockLayer,
                         data,
                         parent,
                         gridSettings.CellSize );
            }
        }

        private void RemoveBlockChunk(Vector3Int chunkCoord, Transform parent)
        {
            var chunkTransform = parent.Find( GetChunkName( chunkCoord ) );
            if (!chunkTransform) {
                throw new LevelAPIException( $"Missing chunk {chunkCoord}" );
            }
            // Все дочерние объекты должны утилизироваться с помощью специального метода
            for (int i = 0; i < chunkTransform.childCount; i++) {
                var subObj = chunkTransform.GetChild( i ).gameObject;
                _objViewFabric.Remove( subObj );
            }
            GameObject.Destroy( chunkTransform.gameObject );
        }

        private static string GetChunkName(Vector3Int chunkCoord) => $"{chunkCoord.x}-{chunkCoord.y}-{chunkCoord.z}";

        private struct GameObjectInfo
        {
            public GameObject gameObject;
        }
    }

    public abstract class ViewChunkLayerSyncronizer<TData, TGlobalDataKey>
    {
        private LevelAPI _level;
        protected GridState _gridState;
        protected ChunkLayer<TData, TGlobalDataKey> _dataLayer;
        protected BlockLayer<ClientViewData> _viewDataLayer;
        protected IObjectViewFabric _objViewFabric;

        public ViewChunkLayerSyncronizer(
            LevelAPI level,
            GridState gridState,
            ChunkLayer<TData, TGlobalDataKey> dataLayer,
            IObjectViewFabric objViewFabric)
        {
            _level = level;
            _gridState = gridState;
            _dataLayer = dataLayer;
            _objViewFabric = objViewFabric;
        }

        protected abstract void OnInit();

        protected abstract void OnDestroy();
    }

    /// <summary>
    /// Объект цепляется сверху слоя блоков, чтобы синхронизиовать с ним вьюху
    /// </summary>
    public class BlockLayerSyncronizer : ViewChunkLayerSyncronizer<BlockData, Vector3Int>
    {
        public BlockLayerSyncronizer(
            LevelAPI level,
            GridState gridState,
            ChunkLayer<BlockData, Vector3Int> chunkLayer,
            IObjectViewFabric objViewFabric)
            : base( level, gridState, chunkLayer, objViewFabric ) { }

        protected override void OnInit()
        {
            _viewDataLayer = (BlockLayer<ClientViewData>)_gridState.AddViewLayer( _dataLayer.Tag + "_VIEW", _dataLayer );
            _dataLayer.chunkAdded += OnChunkAdded;
            _dataLayer.chunkRemoved += OnChunkRemoved;
            _dataLayer.changed += OnLayerChanged;
        }

        protected override void OnDestroy()
        {
            _gridState.RemoveViewLayer( _viewDataLayer.Tag );
            _dataLayer.chunkAdded -= OnChunkAdded;
            _dataLayer.chunkRemoved -= OnChunkRemoved;
            _dataLayer.changed -= OnLayerChanged;
        }

        private void OnLayerChanged(DataLayerEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void OnChunkRemoved(Vector3Int chunkCoord)
        {
            throw new NotImplementedException();
        }

        private void OnChunkAdded(Vector3Int chunkCoord)
        {
            throw new NotImplementedException();
        }

        private void SetupBlockChunk(
            Vector3Int chunkCoord,
            BlockLayer<BlockData> blockLayer,
            Transform parent,
            GridSettings gridSettings)
        {
            GameObject chunkView = new( GetChunkName( chunkCoord ) );
            chunkView.transform.parent = parent;
            chunkView.transform.localRotation = Quaternion.identity;
            chunkView.transform.localPosition = new Vector3(
                gridSettings.ChunkSize.x * chunkCoord.x,
                gridSettings.ChunkSize.y * chunkCoord.y,
                gridSettings.ChunkSize.z * chunkCoord.z
                );

            var content = (DataLayerStaticContent<BlockData>)blockLayer.GetChunkData( chunkCoord );

            for (int i = 0; i < content.Size; i++) {
                BlockData data = content[i];
                Vector3Int localBlockCoord = GridState.FlatToBlockCoord( i, blockLayer.ChunkSize );

                AddBlock( blockLayer.BlockGlobalCoord( chunkCoord, i ),
                         blockLayer,
                         data,
                         parent,
                         gridSettings.CellSize );
            }
        }

        private void RemoveBlockChunk(Vector3Int chunkCoord, Transform parent)
        {
            var chunkTransform = parent.Find( GetChunkName( chunkCoord ) );
            if (!chunkTransform) {
                throw new LevelAPIException( $"Missing chunk {chunkCoord}" );
            }
            // Все дочерние объекты должны утилизироваться с помощью специального метода
            for (int i = 0; i < chunkTransform.childCount; i++) {
                var subObj = chunkTransform.GetChild( i ).gameObject;
                _objViewFabric.Remove( subObj );
            }
            GameObject.Destroy( chunkTransform.gameObject );
        }

        private void AddBlock(
            Vector3Int globalBlockCoord,
            BlockLayer<BlockData> blockLayer,
            BlockData blockData,
            Transform layerRoot,
            Vector3 cellSize)
        {
            var localBlockCoord = blockLayer.LocalCoordOfGlobalBlock( globalBlockCoord );
            var chunkCoord = blockLayer.GetChunkOfGlobalBlock( globalBlockCoord );

            var chunkRoot = layerRoot.Find( GetChunkName( chunkCoord ) );
            if (!chunkRoot) {
                throw new Exception( $"Not found chunk root {chunkCoord}" );
            }

            if (blockData.blockId == 0) {
                throw new LevelAPIException( $"Zero block id {layerRoot} {localBlockCoord}" );
            }

            Vector3 pos = new Vector3(
                localBlockCoord.x * cellSize.x,
                localBlockCoord.y * cellSize.y,
                localBlockCoord.z * cellSize.z );
            BlockProto blockProto = _levelAPI.BlockProtoCollection[blockData.blockId];
            var objectView = _objViewFabric.Create( blockProto.Name );
            objectView.transform.parent = layerRoot;
            objectView.transform.localRotation = BlockData.DecodeRotation( blockData.rotation );
            objectView.transform.localPosition = pos;

            GameObjectInfo goInfo = new() { };
            _blocksGlobalNavi.Add( globalBlockCoord, goInfo );
        }

        private static string GetChunkName(Vector3Int chunkCoord) => $"{chunkCoord.x}-{chunkCoord.y}-{chunkCoord.z}";
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
            handler( value );
        }
        return action;
    }

    public static Action<TValue1, TValue2> SubscribeCollection<TValue1, TValue2>(
        IEnumerable<TValue1> collection,
        Action<TValue1, TValue2> action,
        TValue2 value2,
        Action<TValue1, TValue2> handler)
    {
        action += handler;
        foreach (TValue1 value in collection) {
            handler( value, value2 );
        }
        return action;
    }
}