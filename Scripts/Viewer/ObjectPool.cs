using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    /// <summary>
    /// Пул объектов изначально заданного идентификатора
    /// </summary>
    public class ObjectPool
    {
        //TODO Прогрев пула (прединициализация)
        private string _prefabId;
        private Stack<GameObject> _stack = new();
        private ConstructFabric _fabric;
        private Transform _root;

        public ObjectPool(string prefabId, ConstructFabric fabric, Transform root)
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

        public GameObject Pop()
        {
            if (_stack.Count > 0) {
                return _stack.Pop();
            } else {
                return _fabric.Create( _prefabId );
            }
        }

        public void Push(GameObject obj)
        {
            obj.transform.parent = _root;
            _stack.Push( obj );
        }
    }
}