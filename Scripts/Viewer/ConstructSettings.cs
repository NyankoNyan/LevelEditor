using System;
using UnityEngine;

namespace LevelView
{
    [CreateAssetMenu( fileName = "ConstructSettings", menuName = "LevelEditor/ConstructSettings" )]
    public class ConstructSettings : ScriptableObject
    {
        [SerializeField] Prefab[] _prefabs;

        [Serializable]
        private struct Prefab
        {
            public string id;
            public ObjectView prefab;
        }

        public IConstructFabric GetConstructFabric()
        {
            ConstructFabric constructFabric = new();
            for (int i = 0; i < _prefabs.Length; i++) {
                var prefab = _prefabs[i];
                try {
                    constructFabric.AddPrefab( prefab.id, prefab.prefab );
                } catch (Exception e) {
                    Debug.LogError( $"Index {i}: {e.Message}" );
                }
            }

            return constructFabric;
        }
    }
}
