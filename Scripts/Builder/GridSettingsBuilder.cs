using Level.API;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Builder
{
    public class GridSettingsBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] List<GridSettingsCore> _gridsSettings = new();

        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<LevelBuilder>()) {
                Debug.LogError( $"{this}: Required {nameof( LevelBuilder )} as parent" );
            }
        }

        public void Export(IGridSettingsAPI api)
        {
            foreach (var gs in _gridsSettings) {
                api.Add( gs );
            }
        }

        public void Import(IGridSettingsAPI api)
        {
            _gridsSettings.Clear();
            foreach (var gs in api.Values) {
                _gridsSettings.Add( gs.Settings );
            }
        }

    }
}
