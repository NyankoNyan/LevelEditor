using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    [Serializable]
    public struct GridSettingsCore
    {
        public string name;
        public Vector3Int chunkSize;
        public Vector3 cellSize;
        public string formFactor;
        public bool hasBounds;
        public GridBoundsRect bounds;
        public List<DataLayerSettings> layers;
    }

    public enum LayerType
    {
        BlockLayer,
        PropsLayer
    }

    [Serializable]
    public struct DataLayerSettings
    {
        public string name;
        public LayerType layerType;
        public string tag;
    }

    public class GridSettings : IHasKey<uint>, IInitializable<GridSettingsCore>, IDestroy
    {
        public UnityAction changed;
        public UnityAction<DataLayerSettings> layerAdded;
        public UnityAction<DataLayerSettings> layerRemoved;

        private uint _id;
        private GridSettingsCore _settings;

        public uint Key => _id;

        public string Name
        {
            get => _settings.name;
            set {
                if (value != _settings.name) {
                    _settings.name = value;
                    changed?.Invoke();
                }
            }
        }

        public GridSettingsCore Settings => _settings;

        public Vector3Int ChunkSize
        {
            get => _settings.chunkSize;
            set {
                if (value != _settings.chunkSize) {
                    _settings.chunkSize = value;
                    changed?.Invoke();
                }
            }
        }

        public Vector3 CellSize
        {
            get => _settings.cellSize;
            set {
                if (value != _settings.cellSize) {
                    _settings.cellSize = value;
                    changed?.Invoke();
                }
            }
        }

        public string FormFactor
        {
            get => _settings.formFactor;
            set {
                if (value != _settings.formFactor) {
                    _settings.formFactor = value;
                    changed?.Invoke();
                }
            }
        }

        public int ChunkSizeFlat => ChunkSize.x * ChunkSize.y * ChunkSize.z;
        public UnityAction OnDestroyAction { get; set; }

        public void Destroy()
        {
            if (OnDestroyAction != null) {
                OnDestroyAction();
            }
        }

        public void Init(GridSettingsCore value, uint counter)
        {
            _settings = value;
            _id = counter;
        }

        public void AddLayer(DataLayerSettings layerSettings)
        {
            if (_settings.layers.Any( x => x.tag == layerSettings.tag )) {
                throw new Exception( $"Layer with tag {layerSettings.tag} already added" );
            }

            _settings.layers.Add( layerSettings );

            layerAdded?.Invoke( layerSettings );
            changed?.Invoke();
        }

        public void RemoveLayer(string layerTag)
        {
            int removeIndex = _settings.layers.FindIndex( x => x.tag == layerTag );
            if (removeIndex == -1) {
                throw new ArgumentException( $"Don't found layer {layerTag}" );
            }
            DataLayerSettings layerSettings = _settings.layers[removeIndex];
            _settings.layers.RemoveAt( removeIndex );

            layerRemoved?.Invoke( layerSettings );
            changed?.Invoke();
        }
    }

    [Serializable]
    public struct GridBoundsRect
    {
        public Vector3Int chunkFrom;
        public Vector3Int chunkTo;

        public GridBoundsRect(Vector3Int chunkFrom, Vector3Int chunkTo)
        {
            this.chunkFrom = chunkFrom;
            this.chunkTo = chunkTo;
        }
    }

    public class GridSettingsRegistry : Registry<uint, GridSettings>
    { };

    public class GridSettingsFabric : Fabric<GridSettings, GridSettingsCore>
    { };
}