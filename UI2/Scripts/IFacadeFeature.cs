using UnityEngine;

namespace UI2
{
    public interface IFacadeFeature
    {
        void Use(string action, params object[] actonParams);
        void Init(GameObject go, IElementInstance instance);
        void Enable();
        void Disable();
    }
}