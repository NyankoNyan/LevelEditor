using Level.API;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Builder
{
    public class GridSettingsBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] private List<GridSettingsCreateParams> _gridsSettings = new();

        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<LevelBuilder>()) {
                Debug.LogError( $"{this}: Required {nameof( LevelBuilder )} as parent" );
            }
        }

        public void Export(GridSettingsCollection gridSettingsCollection)
        {
            foreach (var gs in _gridsSettings) {
                gridSettingsCollection.Add( gs );
            }
        }

        public void Import(GridSettingsCollection gridSettingsCollection)
        {
            _gridsSettings.Clear();
            foreach (var gs in gridSettingsCollection) {
                _gridsSettings.Add( gs.Settings );
            }
        }
    }
}