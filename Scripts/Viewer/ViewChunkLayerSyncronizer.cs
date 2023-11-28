using Level;
using Level.API;

using UnityEngine;

namespace LevelView
{
    public abstract class ViewChunkLayerSyncronizer<TData, TGlobalDataKey>
    {
        protected LevelAPI _level;
        protected GridState _gridState;
        protected ChunkLayer<TData, TGlobalDataKey> _chunkLayer;
        protected BlockLayer<ClientViewData> _viewDataLayer;
        protected IObjectViewFabric _objViewFabric;
        protected Transform _gridTransform;


        public ViewChunkLayerSyncronizer(
            LevelAPI level,
            GridState gridState,
            ChunkLayer<TData, TGlobalDataKey> chunkLayer,
            IObjectViewFabric objViewFabric,
            Transform gridTransform)
        {
            _level = level;
            _gridState = gridState;
            _chunkLayer = chunkLayer;
            _objViewFabric = objViewFabric;
            _gridTransform = gridTransform;
        }

        public void Init() => OnInit();
        public void Destroy() => OnDestroy();

        protected abstract void OnInit();

        protected abstract void OnDestroy();
    }
}
