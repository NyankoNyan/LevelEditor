using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    /// <summary>
    /// Хранилище всех объектов с идентификаторами.
    /// </summary>
    public class ConstructMetaStorage
    {
        //TODO Добавить глобальный прогрев объектов игры
        private Dictionary<string, ObjectPool> _pools = new();
        private ConstructFabric _constructFabric;

        public ConstructMetaStorage(ConstructFabric constructFabric)
        {
            _constructFabric = constructFabric ?? throw new ArgumentNullException( nameof( constructFabric ) );
        }

        public GameObject Pop(string prefabId)
        {
            ObjectPool pool;
            if (!_pools.TryGetValue( prefabId, out pool )) {
                GameObject poolRoot = new( $"Pool {prefabId}" );
                pool = new ObjectPool( prefabId, _constructFabric, poolRoot.transform );
                _pools.Add( prefabId, pool );
            }

            return pool.Pop();
        }

        public void Push(string prefabId, GameObject obj)
        {
            var pool = _pools[prefabId];
            pool.Push( obj );
        }

        public bool IsLoaded(string prefabId)
            => _pools.ContainsKey( prefabId );
    }
}