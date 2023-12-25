using Level.API;
using UnityEngine;

namespace LevelView
{
    public class LevelViewerEditor : MonoBehaviour
    {
        [SerializeField] private ConstructSettings _constructSettings;

        public void SetModel(LevelAPI levelAPI)
        {
            while (transform.childCount > 0) {
                DestroyImmediate( transform.GetChild( 0 ).gameObject );
            }
            LevelViewBuilder builder = new( _constructSettings.GetConstructFabric() );
            builder.Build( levelAPI, transform, true );
        }
    }
}