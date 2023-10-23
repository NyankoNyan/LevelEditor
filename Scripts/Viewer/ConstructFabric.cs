using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    public interface IConstructFabric
    {
        ObjectView Create(string prefabId);
        bool HasPrefab(string prefabId);
    }

    public class ConstructFabric : IConstructFabric
    {
        private Dictionary<string, ObjectView> _prefabs = new();
        public void AddPrefab(string prefabId, ObjectView go)
            => _prefabs.Add( prefabId, go );

        public ObjectView Create(string prefabId)
        {
            var prefab = _prefabs[prefabId];
            return GameObject.Instantiate( prefab );
        }

        public bool HasPrefab(string prefabId)
            => _prefabs.ContainsKey( prefabId );
    }
}
