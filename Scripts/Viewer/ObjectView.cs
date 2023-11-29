using System.Collections.Generic;

using Level.API;

using UnityEngine;

namespace LevelView
{
//TODO Rename file

    /// <summary>
    /// Возвращает новые (или хорошо забытые старые) монобехи, запрошенные по идентификатору
    /// </summary>
    public interface IObjectViewFabric
    {
        GameObject Create(string prefabId);

        void Remove(GameObject gameObject);

        bool HasPrefab(string prefabId);

        /// <summary>
        /// Помечает префаб как неиспользуемый
        /// </summary>
        /// <param name="prefabId"></param>
        void Unuse(string prefabId);
    }

    public class ObjectViewFabric : IObjectViewFabric
    {
        private readonly ConstructFabric _constructFabric;
        private readonly ConstructMetaStorage _constructMetaStorage;
        private readonly Dictionary<GameObject, string> _objectReg = new();

        public ObjectViewFabric(ConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
            _constructMetaStorage = new(constructFabric);
        }

        public GameObject Create(string prefabId)
        {
            GameObject obj = _constructMetaStorage.Pop(prefabId);
            return obj;
        }

        public bool HasPrefab(string prefabId)
        {
            return _constructFabric.HasRefId(prefabId);
        }


        public bool IsLoaded(string prefabId)
            => _constructMetaStorage.IsLoaded(prefabId);

        public void Remove(GameObject gameObject)
        {
            if (_objectReg.TryGetValue(gameObject, out string setupId)) {
                _objectReg.Remove(gameObject);
                _constructMetaStorage.Push(setupId, gameObject);
            } else {
                throw new LevelAPIException($"Not found registration for object {gameObject} ");
            }
        }

        public void Unuse(string prefabId)
        {
            _constructMetaStorage.RemovePool(prefabId);
        }

    }

    /// <summary>
    /// For editor
    /// </summary>
    public class ObjectViewFabricNonPool : IObjectViewFabric
    {
        private readonly ConstructFabric _constructFabric;

        public ObjectViewFabricNonPool(ConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
        }

        public GameObject Create(string prefabId)
        {
            return _constructFabric.Create(prefabId);
        }

        public bool HasPrefab(string prefabId)
        {
            return _constructFabric.HasRefId(prefabId);
        }


        public void Remove(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
        }

        public void Unuse(string prefabId)
        {

        }
    }
}