using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    public class ConstructFabric 
    {
        private Dictionary<string, GameObject> _prefabs = new();
        public void AddPrefab(string prefabId, GameObject go)
            => _prefabs.Add( prefabId, go );

        public GameObject Create(string prefabId)
        {
            var prefab = _prefabs[prefabId];
            return GameObject.Instantiate( prefab );
        }

        public bool HasPrefab(string prefabId)
            => _prefabs.ContainsKey( prefabId );
    }
}
