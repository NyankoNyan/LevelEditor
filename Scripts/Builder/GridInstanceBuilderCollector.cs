using Level.API;
using UnityEngine;

namespace Level.Builder
{
    public class GridInstanceBuilderCollector : MonoBehaviour, IBuilderCheck
    {
        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<LevelBuilder>()) {
                Debug.LogError( $"{this}: Required {nameof( LevelBuilder )} as parent" );
            }
        }

        public void Export(GridStatesCollection gridStatesCollection, BlockProtoCollection blockProtos)
        {
            var gridStateBuilders = transform.GetComponentsInChildren<GridInstanceBuilder>();
            foreach (var gsb in gridStateBuilders) {
                gsb.Export( gridStatesCollection, blockProtos );
            }
        }

        public void Import(GridStatesCollection gridStatesCollection, BlockProtoCollection blockProtos)
        {
            Clear();
            foreach (var gridState in gridStatesCollection) {
                var go = new GameObject( gridState.Key.ToString() );
                go.transform.parent = transform;
                var gridInstance = go.AddComponent<GridInstanceBuilder>();
                gridInstance.Import( gridState, blockProtos );
            }
        }

        private void Clear()
        {
            var gridStateBuilders = transform.GetComponentsInChildren<GridInstanceBuilder>();
            foreach (var gsb in gridStateBuilders) {
                DestroyImmediate( gsb.gameObject );
            }
        }
    }
}