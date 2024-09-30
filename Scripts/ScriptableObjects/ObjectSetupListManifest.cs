using UnityEngine;

namespace LevelView
{
    /// <summary>
    /// Список отображаемых объектов сцены
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectSetupList", menuName = "LevelEditor/ObjectSetupList")]
    public class ObjectSetupListManifest : ScriptableObject
    {
        [SerializeField] public ObjectSetup[] Objects;
    }
}