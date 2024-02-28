using UnityEngine;

namespace UI2
{
    [DefaultExecutionOrder(-1)]
    public class StylesScenePlugin : MonoBehaviour
    {
        [SerializeField] private Style[] _styles;

        private void Start()
        {
            foreach (var errStyle in UIProvider.Get().Reg(_styles)) {
                Debug.LogError($"Error with style {errStyle.name}");
            }
        }
    }
}