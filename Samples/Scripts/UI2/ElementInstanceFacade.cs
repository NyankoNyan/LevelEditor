using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UI2
{
    public class ElementInstanceFacade : MonoBehaviour
    {
        [SerializeField] private Transform _subZone;
        private readonly List<IFacadeFeature> _features = new();

        public IElementInstance ElementInstance { get; internal set; }

        public Transform SubZone => _subZone ?? transform;

        public T GetFeature<T>() where T : class, IFacadeFeature
            => _features.SingleOrDefault(f => f is T) as T;

        protected void AddFeature(params IFacadeFeature[] features)
            => _features.AddRange(features);

        private void Start()
        {
            foreach (var feature in _features) {
                try {
                    feature.Init(gameObject, ElementInstance);
                } catch (ElementWorkflowException) {
                    Debug.LogError($"Error on feature activation {feature.GetType().Name}");
                }
            }
        }

        private void OnEnable()
        {
            foreach (var feature in _features) {
                feature.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (var feature in _features) {
                feature.Disable();
            }
        }
    }
}