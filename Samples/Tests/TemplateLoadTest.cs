using System;

using LevelView;

using UnityEngine;
using UnityEngine.Assertions;

namespace Level.Samples
{
    public class TemplateLoadTest : MonoBehaviour
    {
        [SerializeField] private SingleTemplateLoad _loadStarter;
        [SerializeField] private LevelStorage _levelStorage;
        [SerializeField] private ObjectSetupListManifest _blockViews;
        [SerializeField] private BlockProtoListManifest _blockPrototypes;
        [SerializeField] private GridSettingsListManifest _gridSettings;

        private readonly ConstructFabric _constructFabric = new();
        private LevelViewBuilder _levelViewBuilder;

        private void Awake()
        {
            Assert.IsNotNull(_loadStarter);
            Assert.IsNotNull(_levelStorage);
            Assert.IsNotNull(_blockViews);
            Assert.IsNotNull(_blockPrototypes);
            Assert.IsNotNull(_gridSettings);
        }

        private void Start()
        {
            LoadBlockPrototypes();
            LoadGrids();
            LoadTemplate();
            InitView();
        }

        private void LoadBlockPrototypes()
        {
            foreach (var objectViewSetup in _blockViews.Objects) {
                _constructFabric.AddPrefab(objectViewSetup);
            }

            foreach (var blockProtoSettings in _blockPrototypes.Blocks) {
                var blockProto = _levelStorage.API.BlockProtoCollection.Add(blockProtoSettings);
                if (!_constructFabric.HasRefId(blockProto.Name)) {
                    throw new Exception($"Missing block view for {blockProto.Name}");
                }
            }
        }

        private void LoadGrids()
        {
            if (_gridSettings.GridSettingsList.Length == 0) {
                throw new Exception("Empty grid settings");
            }

            bool first = true;
            foreach (var gridSettingsCP in _gridSettings.GridSettingsList) {
                var gridSettings = _levelStorage.API.GridSettingsCollection.Add(gridSettingsCP);
                _levelStorage.API.GridStatesCollection.Add(gridSettings.Key);
                if (first) {
                    first = false;
                }
            }
        }

        private void LoadTemplate()
        {
            _loadStarter.Load(_levelStorage.API);
        }

        private void InitView()
        {
            _levelViewBuilder = new(_constructFabric);
            _levelViewBuilder.Build(_levelStorage.API, new GameObject("Grids").transform, false);
        }
    }
}