using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level.API
{
    public interface IGridSettingsAPI
    {
        uint Add(GridSettingsCore gridSettings);

        GridSettings Add(GridSettingsCore gridSettings, uint id);

        void Remove(uint gridSettingsId);

        IEnumerable<GridSettings> Values { get; }

        GridSettings GetGridSettings(string name);

        GridSettings GetGridSettings(uint gridSettingsId);

        uint AddEmpty();
    }

    public class GridSettingsAPI : IGridSettingsAPI
    {
        private GridSettingsRegistry _gridSettingsRegistry;
        private GridSettingsFabric _gridSettingsFabric;

        public GridSettingsAPI(
            GridSettingsRegistry gridSettingsRegistry,
            GridSettingsFabric gridSettingsFabric
            )
        {
            _gridSettingsRegistry = gridSettingsRegistry;
            _gridSettingsFabric = gridSettingsFabric;
        }

        public IEnumerable<GridSettings> Values => _gridSettingsRegistry.Values;

        public uint Add(GridSettingsCore gridSettings)
        {
            if (string.IsNullOrWhiteSpace( gridSettings.name )) {
                throw new LevelAPIException( $"Empty grid settings name" );
            }
            if (_gridSettingsRegistry.Values.Any( x => x.Name == gridSettings.name )) {
                throw new LevelAPIException( $"Grid settings with name {gridSettings.name} already exists" );
            }
            return _gridSettingsFabric.Create( gridSettings ).Key;
        }

        public GridSettings Add(GridSettingsCore gridSettings, uint id)
        {
            if (string.IsNullOrWhiteSpace( gridSettings.name )) {
                throw new LevelAPIException( $"Empty grid settings name" );
            }
            if (id == 0) {
                throw new LevelAPIException( $"Empty grid settings id" );
            }
            if (_gridSettingsRegistry.Values.Any( x => x.Name == gridSettings.name )) {
                throw new LevelAPIException( $"Grid settings with name {gridSettings.name} already exists" );
            }
            return _gridSettingsFabric.CreateWithCounter( gridSettings, id );
        }

        public uint AddEmpty()
        {
            //Create name
            const int nameTryCount = 10;
            string baseName = $"grid_{_gridSettingsFabric.Counter + 1}";

            GridSettingsCore gridSettings = new() {
                chunkSize = Vector3Int.one,
                formFactor = "block1x1x1",
                hasBounds = false,
                cellSize = Vector3.one,
                layers = { new() {
                    layerType = LayerType.BlockLayer,
                    tag = "block"
                } }
            };

            try {
                gridSettings.name = baseName;
                return Add( gridSettings );
            } catch (LevelAPIException) {
                for (int i = 0; i < nameTryCount; i++) {
                    try {
                        gridSettings.name = baseName + UnityEngine.Random.Range( 0, 10000 );
                        return Add( gridSettings );
                    } catch (LevelAPIException) { }
                }
                throw new LevelAPIException( $"Cant't find free name for new block proto" );
            }
        }

        public GridSettings GetGridSettings(string name)
        {
            try {
                return _gridSettingsRegistry.Values.First( x => x.Name == name );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing grid with name {name}" );
            }
        }

        public GridSettings GetGridSettings(uint gridSettingsId)
            => _gridSettingsRegistry.Dict[gridSettingsId];

        public void Remove(uint gridSettingsId)
        {
            GridSettings gridSettings = null;
            try {
                gridSettings = _gridSettingsRegistry.Values.First( x => x.Key == gridSettingsId );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing grid with id {gridSettingsId}" );
            }
        }
    }
}