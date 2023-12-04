using System;
using System.Collections.Generic;

using UnityEngine;

namespace Level
{
    public struct DynamicFieldRights
    {
        public bool read;
        public bool write;
    }

    public interface IDynamicSettings
    {
        public object GetComponentValue(string name);

        public void SetComponentValue(string name, object value);

        public string CheckComponentValue(string name, object value);

        public string GetComponentType(string name);

        public IEnumerable<string> GetComponents();

        public DynamicFieldRights GetUserRights(string userId, string name);

        public Action<string> changed { get; set; }
        public Action locked { get; set; }
        public bool IsLocked { get; }

        public bool TryLock();

        public void Unlock();
    }

    public class BlockProtoDynamicSettings : IDynamicSettings
    {
        public BlockProto BlockProto {
            get => _blockProto;
            set {
                if (_blockProto != null) {
                    _blockProto.changed -= OnChanged;
                }
                _blockProto = value;
                _blockProto.changed += OnChanged;
                _beforeChange = _blockProto.Settings;
            }
        }

        public Action<string> changed { get; set; }
        public Action locked { get; set; }

        public bool IsLocked => false; //Пока блокировок нет

        private BlockProto _blockProto;
        private BlockProtoSettings _beforeChange;

        public IEnumerable<string> GetComponents()
        {
            yield return "tag";
            yield return "name";
            yield return "form_factor";
            yield return "size";
            yield return "lock_x_z";
        }

        public string GetComponentType(string name)
        {
            switch (name) {
                case "tag":
                case "name":
                case "form_factor":
                    return "string";

                case "lock_x_z":
                    return "bool";

                case "size":
                    return "Vector3Int";

                default:
                    return null;
            }
        }

        public object GetComponentValue(string name)
        {
            switch (name) {
                case "tag":
                    return _blockProto.Tag;

                case "name":
                    return _blockProto.Name;

                case "form_factor":
                    return _blockProto.FormFactor;

                case "lock_x_z":
                    return _blockProto.Settings.lockXZ;

                case "size":
                    return _blockProto.Size;

                default:
                    return null;
            }
        }

        public DynamicFieldRights GetUserRights(string userId, string name)
        {
            switch (name) {
                case "tag":
                case "name":
                case "form_factor":
                case "size":
                    return new DynamicFieldRights() { read = true, write = true };

                case "lock_x_z":
                    return new DynamicFieldRights() { read = true };

                default:
                    return default;
            }
        }

        public void SetComponentValue(string name, object value)
        {
            switch (name) {
                case "tag":
                    _blockProto.Tag = (string)value;
                    break;

                case "name":
                    _blockProto.Name = (string)value;
                    break;

                case "form_factor":
                    _blockProto.FormFactor = (string)value;
                    break;

                case "lock_x_z":
                    throw new NotImplementedException();
                case "size":
                    _blockProto.Size = (Vector3Int)value;
                    break;
            }
        }

        private void OnChanged()
        {
            if (changed != null) {
                if (_beforeChange.layerTag != _blockProto.Tag) {
                    changed("tag");
                }
                if (_beforeChange.name != _blockProto.Name) {
                    changed("name");
                }
                if (_beforeChange.size != _blockProto.Size) {
                    changed("size");
                }
                if (_beforeChange.formFactor != _blockProto.FormFactor) {
                    changed("form_factor");
                }
                if (_beforeChange.lockXZ != _blockProto.Settings.lockXZ) {
                    changed("lock_x_z");
                }
            }
        }

        public string CheckComponentValue(string name, object value)
        {
            //TODO check usage before change
            switch (name) {
                case "tag":
                case "name":
                case "form_factor":
                    if (string.IsNullOrWhiteSpace((string)value)) {
                        return $"Empty value of {name}";
                    }
                    break;

                case "size":
                    var size = (Vector3Int)value;
                    if (size.x <= 0 || size.y <= 0 || size.z <= 0) {
                        return $"Components of {name} must be positive";
                    }
                    break;
            }
            return null;
        }

        public bool TryLock()
        {
            return true;
        }

        public void Unlock()
        {
        }
    }
}