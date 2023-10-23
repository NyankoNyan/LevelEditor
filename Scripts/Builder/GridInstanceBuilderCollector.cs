﻿using Level.API;
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

        public void Export(IGridStatesAPI api, IBlockProtoAPI blockProtoAPI)
        {
            var gridStateBuilders = transform.GetComponentsInChildren<GridInstanceBuilder>();
            foreach (var gsb in gridStateBuilders) {
                gsb.Export( api, blockProtoAPI );
            }
        }

        public void Import(IGridStatesAPI api, IBlockProtoAPI blockProtoAPI)
        {
            Clear();
            foreach (var gridState in api.Grids) {
                var go = new GameObject( gridState.Key.ToString() );
                go.transform.parent = transform;
                var gridInstance = go.AddComponent<GridInstanceBuilder>();
                gridInstance.Import( gridState, blockProtoAPI );
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