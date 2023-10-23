using System;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    public class BlockProto : IHasKey<uint>, IInitializable<BlockProtoCreateParams>, IDestroy
    {
        public UnityAction changed;
        public UnityAction<string, string> nameChanged;
        public UnityAction<string, string> tagChanged;
        public UnityAction<string, string> formFactorChanged;

        private uint _id;
        private BlockProtoSettings _settings;

        public uint Key => _id;

        public string Name
        {
            get => _settings.name;
            set {
                if (value != _settings.name) {
                    string oldValue = _settings.name;
                    _settings.name = value;
                    nameChanged?.Invoke( oldValue, value );
                    changed?.Invoke();
                }
            }
        }

        public string Tag
        {
            get => _settings.layerTag;
            set {
                if (value != _settings.layerTag) {
                    string oldValue = _settings.layerTag;
                    _settings.layerTag = value;
                    tagChanged?.Invoke( oldValue, value );
                    changed?.Invoke();
                }
            }
        }

        public string FormFactor
        {
            get => _settings.formFactor;
            set {
                if (value != _settings.formFactor) {
                    string oldValue = _settings.formFactor;
                    _settings.formFactor = value;
                    formFactorChanged?.Invoke( oldValue, value );
                    changed?.Invoke();
                }
            }
        }

        public Vector3Int Size
        {
            get => _settings.size;
            set {
                if (value != _settings.size) {
                    _settings.size = value;
                    changed?.Invoke();
                }
            }
        }

        internal BlockProtoSettings Settings => _settings;

        public UnityAction OnDestroyAction { get; set; }

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
    public struct BlockProtoSettings
    {
        public string name;
        public string formFactor;
        public string layerTag;
        public Vector3Int size;
        public bool lockXZ;
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