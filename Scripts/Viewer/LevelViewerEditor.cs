using Level.API;
using System;
using UnityEngine;

namespace LevelView
{
    public class LevelViewerEditor : MonoBehaviour
    {
        [SerializeField] ConstructSettings _constructSettings;

        public void SetModel(ILevelAPI levelAPI)
        {
            while (transform.childCount > 0) {
                DestroyImmediate( transform.GetChild( 0 ).gameObject );
            }
            LevelViewBuilder builder = new( _constructSettings.GetConstructFabric() );
            builder.Build( levelAPI, transform, true );
        }
    }
}
