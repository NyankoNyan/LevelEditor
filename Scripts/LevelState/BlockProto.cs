﻿using System;

using UnityEngine;

namespace Level
{
    public class BlockProto : IHasKey<uint>, IInitializable<BlockProtoCreateParams>, IDestroy
    {
        public Action changed;

        private uint _id;
        private BlockProtoSettings _settings;

        public uint Key => _id;

        public string Name {
            get => _settings.name;
            set {
                if (value != _settings.name) {
                    string oldValue = _settings.name;
                    _settings.name = value;
                    changed?.Invoke();
                }
            }
        }

        public string Tag {
            get => _settings.layerTag;
            set {
                if (value != _settings.layerTag) {
                    string oldValue = _settings.layerTag;
                    _settings.layerTag = value;
                    changed?.Invoke();
                }
            }
        }

        public string FormFactor {
            get => _settings.formFactor;
            set {
                if (value != _settings.formFactor) {
                    string oldValue = _settings.formFactor;
                    _settings.formFactor = value;
                    changed?.Invoke();
                }
            }
        }

        public Vector3Int Size {
            get => _settings.size;
            set {
                if (value != _settings.size) {
                    _settings.size = value;
                    changed?.Invoke();
                }
            }
        }

        public BlockProtoSettings Settings {
            get => _settings;
            set {
                if (!_settings.Equals(value)) {
                    _settings = value;
                    changed?.Invoke();
                }
            }
        }

        public Action OnDestroyAction { get; set; }

        public BlockProtoInfo Info =>
            new BlockProtoInfo() { id = _id, content = _settings };

        public void Destroy()
        {
            if (OnDestroyAction != null) {
                OnDestroyAction();
            }
        }

        public void Init(BlockProtoCreateParams createParams, uint counter)
        {
            _id = counter;
            _settings = createParams.blockProtoSettings;
        }
    }

    [Serializable]
    public struct BlockProtoSettings : IEquatable<BlockProtoSettings>
    {
        public string name;
        public string formFactor;
        public string layerTag;
        public Vector3Int size;
        public bool lockXZ;

        public bool Equals(BlockProtoSettings other)
        {
            return name == other.name
                && formFactor == other.formFactor
                && layerTag == other.layerTag
                && size == other.size
                && lockXZ == other.lockXZ;
        }
    }

    public struct BlockProtoInfo
    {
        public uint id;
        public BlockProtoSettings content;
    }

    public struct BlockProtoCreateParams
    {
        public BlockProtoSettings blockProtoSettings;

        public BlockProtoCreateParams(BlockProtoSettings blockProtoSettings)
        {
            this.blockProtoSettings = blockProtoSettings;
        }
    }

    public class BlockProtoRegistry : Registry<uint, BlockProto>
    { }

    public class BlockProtoFabric : Fabric<BlockProto, BlockProtoCreateParams>
    { }
}