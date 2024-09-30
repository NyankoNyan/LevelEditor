using System.Linq;

using Level.API;

using LevelTemplates;

using UnityEngine;

namespace Level.Samples
{
    public class SingleTemplateLoad : MonoBehaviour
    {
        [SerializeField] private string _templateFolder = @"Assets\LevelEditor\LevelTemplates\Tests\Templates\Basic";
        [SerializeField] private string _building = "Small house";
        [SerializeField] private string _grid = "default";
        [SerializeField] private string _layer = "default";
        [SerializeField] private Vector3Int _offset;
        [SerializeField] private DiscreteAngle _orientation = DiscreteAngle.U0;

        public void Load(LevelAPI level)
        {
            var loader = new TemplateLoader(_templateFolder);
            var gridState = level.GridStatesCollection.Single(gs => gs.GridSettingsName == _grid);
            loader.LoadToLevel(level, gridState, _layer, _building, _orientation, _offset);
        }
    }
}