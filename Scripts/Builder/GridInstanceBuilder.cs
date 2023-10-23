using Level.API;
using System.Linq;
using UnityEngine;

namespace Level.Builder
{
    public class GridInstanceBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] string gridSettingsName;

        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<GridInstanceBuilderCollector>()) {
                Debug.LogError( $"{this}: Required {nameof( GridInstanceBuilderCollector )} as parent" );
            }
        }

        public void Export(IGridStatesAPI api, IBlockProtoAPI blockProtoAPI)
        {
            var gridState = api.AddState( gridSettingsName );

            foreach (var chunkBuilder in GetComponentsInChildren<ChunkBuilder>()) {
                chunkBuilder.Export( gridState, blockProtoAPI );
            }
        }

        public void Import(GridState gridState, IBlockProtoAPI blockProtoAPI)
        {
            gridSettingsName = gridState.GridSettings.Name;
            transform.name = $"{gridState.Key}-{gridSettingsName}";

            var chunkBuilder = gameObject.AddComponent<ChunkBuilder>();
            chunkBuilder.Import( gridState, blockProtoAPI );
        }
    }
}
