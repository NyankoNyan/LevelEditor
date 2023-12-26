using System;
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

        public (bool, CompareTools.Difference<DataLayerSettings>) Equals(GridSettingsCreateParams other)
        {
            var layersDiff = CompareTools.CompareLists(layers, other.layers);
            return (
                name == other.name
                    && chunkSize == other.chunkSize
                    && cellSize == other.cellSize
                    && formFactor == other.formFactor
                    && hasBounds == other.hasBounds
                    && bounds.Equals(other.bounds)
                    && layersDiff.uniq1.Count() == 0
                    && layersDiff.uniq2.Count() == 0,
                layersDiff
            );
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

        public GridSettingsCreateParams Settings {
            get => _settings;
            set {
                (var equal, var diff) = _settings.Equals(value);
                if (!equal) {
                    _settings = new(value);
                    changed?.Invoke();
                    if (layerRemoved != null) {
                        foreach (var layer in diff.uniq1) {
                            layerRemoved(layer);
                        }
                    }
                    if (layerAdded != null) {
                        foreach (var layer in diff.uniq2) {
                            layerAdded(layer);
                        }
                    }
                }
            }
        }

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

    public static class CompareTools
    {
        public struct Difference<T>
        {
            public IEnumerable<T> uniq1, uniq2;
        }

        public delegate TKey GetKey<T, TKey>(ref T value);

        public static Difference<T> CompareLists<T, TKey>(List<T> list1, List<T> list2, GetKey<T, TKey> getKey)
        {
            Difference<T> difference = new();
            HashSet<TKey> keys1 = new(list1.Select(x => getKey(ref x)));
            HashSet<TKey> keys2 = new(list2.Select(x => getKey(ref x)));
            difference.uniq1 = list1.Where(elem => !keys2.Contains(getKey(ref elem)));
            difference.uniq2 = list2.Where(elem => !keys1.Contains(getKey(ref elem)));
            return difference;
        }

        public static Difference<T> CompareLists<T>(List<T> list1, List<T> list2)
        {
            return CompareLists(list1, list2, (ref T x) => x);
        }
    }
}