using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    public class ObjectPool
    {
        string _prefabId;
        Stack<ObjectView> _stack = new();
        IConstructFabric _fabric;
        Transform _root;

        public ObjectPool(string prefabId, IConstructFabric fabric, Transform root)
        {
            if (string.IsNullOrWhiteSpace( prefabId )) {
                throw new ArgumentException( nameof( prefabId ) );
            }
            _prefabId = prefabId;
            _fabric = fabric ?? throw new ArgumentNullException( nameof( fabric ) );
            _root = root;

            if (!_fabric.HasPrefab( prefabId )) {
                throw new ArgumentException( $"{prefabId} not found" );
            }
        }

        public ObjectView Pop()
        {
            if (_stack.Count > 0) {
                return _stack.Pop();
            } else {
                return _fabric.Create( _prefabId );
            }
        }

        public void Push(ObjectView obj)
        {
            obj.transform.parent = _root;
            _stack.Push( obj );
        }
    }
}
