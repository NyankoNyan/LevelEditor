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
        private string _prefabId;

        private Stack<GameObject> _stack = new();
        private ConstructFabric _fabric;
        private Transform _root;

        public ObjectPool(string prefabId, ConstructFabric fabric, Transform root)
        {
            if (string.IsNullOrWhiteSpace(prefabId)) {
                throw new ArgumentException(nameof(prefabId));
            }
            _prefabId = prefabId;
            _fabric = fabric ?? throw new ArgumentNullException(nameof(fabric));
            _root = root;

            if (!_fabric.HasRefId(prefabId)) {
                throw new ArgumentException($"{prefabId} not found");
            }
        }

        public GameObject Pop()
        {
            if (_stack.Count > 0) {
                var go = _stack.Pop();
                go.SetActive(true);
                return go;
            } else {
                return _fabric.Create(_prefabId);
            }
        }

        public void Push(GameObject obj)
        {
            obj.transform.parent = _root;
            obj.SetActive(false);
            _stack.Push(obj);
        }

        /// <summary>
        /// Прединициалицирует пул указанным количеством объектов 
        /// </summary>
        /// <param name="size"></param>
        public void Prewarm(int size)
        {
            while (_stack.Count < size) {
                var newGO = _fabric.Create(_prefabId);
                newGO.SetActive(false);
                _stack.Push(newGO);
            }
        }

        public void Destroy()
        {
            while (_stack.Count > 0) {
                var go = _stack.Pop();
                GameObject.Destroy(go);
            }
        }
    }
}