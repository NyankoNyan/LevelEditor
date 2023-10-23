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
        ObjectView Create(string prefabId, IObjectViewReceiver receiver);
    }

    public class ObjectViewFabric : IObjectViewFabric
    {
        ConstructMetaStorage _constructMetaStorage;

        public ObjectViewFabric(IConstructFabric constructFabric)
        {
            _constructMetaStorage = new( constructFabric );
        }

        public ObjectView Create(string prefabId, IObjectViewReceiver receiver)
        {
            ObjectView obj = _constructMetaStorage.Pop( prefabId );
            obj.Init( receiver );

            UnityAction onRemove = null;
            onRemove = () => {
                _constructMetaStorage.Push( prefabId, obj );
                receiver.removed -= onRemove;
            };
            receiver.removed += onRemove;

            return obj;
        }

        public void CreateAsync(string prefabId, UnityAction<ObjectView> callback)
        {
            throw new NotImplementedException();
        }

        public bool IsLoaded(string prefabId)
            => _constructMetaStorage.IsLoaded( prefabId );
    }


    /// <summary>
    /// For editor
    /// </summary>
    public class ObjectViewFabricNonPool : IObjectViewFabric
    {
        private IConstructFabric _constructFabric;

        public ObjectViewFabricNonPool(IConstructFabric constructFabric)
        {
            _constructFabric = constructFabric;
        }

        public ObjectView Create(string prefabId, IObjectViewReceiver receiver)
        {
            return _constructFabric.Create( prefabId );
        }
    }


    public class ObjectView : MonoBehaviour
    {
        private IObjectViewReceiver _receiver;
        public IObjectViewReceiver Receiver => _receiver;

        public void Init(IObjectViewReceiver receiver)
        {
            if (receiver == null) {
                throw new ArgumentNullException();
            }
            _receiver = receiver;

            //UnityAction<bool> applyVisibility = (visible) => {
            //    gameObject.SetActive( visible );
            //};

            UnityAction remove = null;
            remove = () => {
                _receiver = null;
                //_receiver.visibilityChanged -= applyVisibility;
                _receiver.removed -= remove;
            };


            //_receiver.visibilityChanged += applyVisibility;
            //applyVisibility( _receiver.Visible );

            _receiver.removed += remove;
        }
    }

}
