using UnityEngine;

namespace UI2
{
    public class ElementInstanceFacade : MonoBehaviour
    {
        [SerializeField] private Transform _subZone;

        public Transform SubZone => _subZone ?? transform;
    }
}