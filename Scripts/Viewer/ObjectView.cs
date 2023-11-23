using Level.API;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LevelView
{
    /// <summary>
    /// Возвращает новенькие монобехи, запрошенные по идентификатору
    /// </summary>
    public interface IObjectViewFabric
    {
        // GameObject Create(string prefabId, IObjectViewReceiver receiver);
        GameObject Create(string prefabId);
    }

    public class ObjectViewFabric : IObjectViewFabric
    {
        private ConstructMetaStorage _constructMetaStorage;

        public ObjectViewFabric(ConstructFabric constructFabric)
        {
            _constructMetaStorage = new( constructFabric );
        }

        // public GameObject Create(string prefabId, IObjectViewReceiver receiver)
        // {
        //     GameObject obj = _constructMetaStorage.Pop( prefabId );
        //     obj.Init( receiver );

        //     Action onRemove = null;
        //     onRemove = () => {
        //         _constructMetaStorage.Push( prefabId, obj );
        //         receiver.removed -= onRemove;
        //     };
        //     receiver.removed += onRemove;

        //     return obj;
        // }

        public GameObject Create(string prefabId)
        {
            GameObject obj = _constructMetaStorage.Pop( prefabId );
            return obj;
        }

        //public void CreateAsync(string prefabId, Action<GameObject> callback)
        //{
        //    throw new NotImplementedException();
        //}

        public bool IsLoaded(string prefabId)
            => _constructMetaStorage.IsLoaded( prefabId );
    }

    /// <summary>
    /// For editor
    /// </summary>
    public class ObjectViewFabricNonPool : IObjectViewFabric
    {
        private ConstructFabric _constructFabric;

        public ObjectViewFabricNonPool(ConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
        }

        public GameObject Create(string prefabId)
        {
            return _constructFabric.Create( prefabId );
        }
    }

    //public class ObjectView : MonoBehaviour
    //{
    //    private IObjectViewReceiver _receiver;
    //    public IObjectViewReceiver Receiver => _receiver;

    //    public void Init(IObjectViewReceiver receiver)
    //    {
    //        if (receiver == null) {
    //            throw new ArgumentNullException();
    //        }
    //        _receiver = receiver;

    //        //UnityAction<bool> applyVisibility = (visible) => {
    //        //    gameObject.SetActive( visible );
    //        //};

    //        Action remove = null;
    //        remove = () => {
    //            _receiver = null;
    //            //_receiver.visibilityChanged -= applyVisibility;
    //            _receiver.removed -= remove;
    //        };

    //        //_receiver.visibilityChanged += applyVisibility;
    //        //applyVisibility( _receiver.Visible );

    //        _receiver.removed += remove;
    //    }
    //}
}