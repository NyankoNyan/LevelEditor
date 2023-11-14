using Level.API;
using UnityEngine;

namespace Level.Builder
{
    public class GridInstanceBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] private string gridSettingsName;

        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<GridInstanceBuilderCollector>()) {
                Debug.LogError( $"{this}: Required {nameof( GridInstanceBuilderCollector )} as parent" );
            }
        }

        public void Export(GridStatesCollection api, BlockProtoCollection blockProtos)
        {
            var gridSettings = api.Level.GridSettingsCollection.FindByName( gridSettingsName );
            var gridState = api.Add( gridSettings.Key );

            foreach (var chunkBuilder in GetComponentsInChildren<ChunkBuilder>()) {
                chunkBuilder.Export( gridState, blockProtos );
            }
        }

        public void Import(GridState gridState, BlockProtoCollection blockProtos)
        {
            gridSettingsName = gridState.GridSettings.Name;
            transform.name = $"{gridState.Key}-{gridSettingsName}";

            var chunkBuilder = gameObject.AddComponent<ChunkBuilder>();
            chunkBuilder.Import( gridState, blockProtos );
        }
    }
}