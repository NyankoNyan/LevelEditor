﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Level
{
    [Serializable]
    public struct GridSettingsCreateParams
    {
        public string name;
        public Vector3Int chunkSize;
        public Vector3 cellSize;
        public string formFactor;
        public bool hasBounds;
        public GridBoundsRect bounds;
        public List<DataLayerSettings> layers;

        public GridSettingsCreateParams(GridSettingsCreateParams other)
        {
            this = other;
            layers = new(other.layers);
        }
    }

    public struct GridSettingsInfo
    {
        public uint id;
        public GridSettingsCreateParams content;
    }

    public enum LayerType
    {
        BlockLayer = 0,
        BigBlockLayer = 1,
        PropsLayer = 2,
        ItemLayer,
        GlobalItemLayer
    }

    //[Serializable]
    //public struct DataLayerSettings
    //{
    //    public string name;
    //    public LayerType layerType;
    //    public string tag;
    //}

    public class GridSettings : IHasKey<uint>, IInitializable<GridSettingsCreateParams>, IDestroy
    {
        public Action changed;
        public Action<DataLayerSettings> layerAdded;
        public Action<DataLayerSettings> layerRemoved;

        private uint _id;
        private GridSettingsCreateParams _settings;

        public uint Key => _id;

        public string Name {
            get => _settings.name;
            set {
                if (value != _settings.name) {
                    _settings.name = value;
                    changed?.Invoke();
                }
            }
        }

        public GridSettingsCreateParams Settings => _settings;

        public Vector3Int ChunkSize {
            get => _settings.chunkSize;
            set {
                if (value != _settings.chunkSize) {
                    _settings.chunkSize = value;
                    changed?.Invoke();
                }
            }
        }

        public Vector3 CellSize {
            get => _settings.cellSize;
            set {
                if (value != _settings.cellSize) {
                    _settings.cellSize = value;
                    changed?.Invoke();
                }
            }
        }

        public string FormFactor {
            get => _settings.formFactor;
            set {
                if (value != _settings.formFactor) {
                    _settings.formFactor = value;
                    changed?.Invoke();
                }
            }
        }

        public int ChunkSizeFlat => ChunkSize.x * ChunkSize.y * ChunkSize.z;
        public Action OnDestroyAction { get; set; }


        public GridSettingsInfo Info => new GridSettingsInfo {
            id = Key,
            content = new(Settings)
        };

        public void Destroy()
        {
            if (OnDestroyAction != null) {
                OnDestroyAction();
            }
        }

        public void Init(GridSettingsCreateParams value, uint counter)
        {
            _settings = value;
            _id = counter;
        }

        public void AddLayer(DataLayerSettings layerSettings)
        {
            if (_settings.layers.Any(x => x.tag == layerSettings.tag)) {
                throw new Exception($"Layer with tag {layerSettings.tag} already added");
            }

            _settings.layers.Add(layerSettings);

            layerAdded?.Invoke(layerSettings);
            changed?.Invoke();
        }

        public void RemoveLayer(string layerTag)
        {
            int removeIndex = _settings.layers.FindIndex(x => x.tag == layerTag);
            if (removeIndex == -1) {
                throw new ArgumentException($"Don't found layer {layerTag}");
            }
            DataLayerSettings layerSettings = _settings.layers[removeIndex];
            _settings.layers.RemoveAt(removeIndex);

            layerRemoved?.Invoke(layerSettings);
            changed?.Invoke();
        }
    }

    [Serializable]
    public struct GridBoundsRect : IEquatable<GridBoundsRect>
    {
        public Vector3Int chunkFrom;
        public Vector3Int chunkTo;

        public GridBoundsRect(Vector3Int chunkFrom, Vector3Int chunkTo)
        {
            this.chunkFrom = chunkFrom;
            this.chunkTo = chunkTo;
        }

        public bool Equals(GridBoundsRect other)
        {
            return this.chunkFrom == other.chunkFrom
                && this.chunkTo == other.chunkTo;
        }


        public override string ToString()
        {
            return $"{{ {chunkFrom} : {chunkTo} }}";
        }
    }

    public class GridSettingsRegistry : Registry<uint, GridSettings>
    { };

    public class GridSettingsFabric : Fabric<GridSettings, GridSettingsCreateParams>
    { };
}