﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    public class ConstructMetaStorage
    {
        Dictionary<string, ObjectPool> _pools = new();
        IConstructFabric _constructFabric;

        public ConstructMetaStorage(IConstructFabric constructFabric)
        {
            _constructFabric = constructFabric ?? throw new ArgumentNullException( nameof( constructFabric ) );
        }

        public ObjectView Pop(string prefabId)
        {
            ObjectPool pool;
            if (!_pools.TryGetValue( prefabId, out pool )) {
                GameObject poolRoot = new( $"Pool {prefabId}" );
                pool = new ObjectPool( prefabId, _constructFabric, poolRoot.transform );
                _pools.Add( prefabId, pool );
            }

            return pool.Pop();
        }

        public void Push(string prefabId, ObjectView obj)
        {
            var pool = _pools[prefabId];
            pool.Push( obj );
        }

        public bool IsLoaded(string prefabId)
            => _pools.ContainsKey( prefabId );
    }
}
