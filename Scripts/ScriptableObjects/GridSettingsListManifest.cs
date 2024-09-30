using Level;

using UnityEngine;

namespace LevelView
{
    [CreateAssetMenu(fileName = "GridSettingsList", menuName = "LevelEditor/GridSettingsList")]
    public class GridSettingsListManifest : ScriptableObject
    {
        [SerializeField] public GridSettingsCreateParams[] GridSettingsList;
    }
}