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

    public interface IDynamicWrapper
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

    public class BlockProtoDynamicWrapper : IDynamicWrapper
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

    public class GridSettingsDynamicWrapper : DynamicWrapperBase
    {
        public GridSettings GridSettings {
            get => _gridSettings;
            set {
                if (_gridSettings != null) {
                    _gridSettings.changed -= OnChanged;
                }
                _gridSettings = value;
                if (_gridSettings != null) {
                    _gridSettings.changed += OnChanged;
                }
                InvokeChangedFields();
            }
        }

        private GridSettings _gridSettings;
        private GridSettingsInfo _prevValue;

        public GridSettingsDynamicWrapper()
        {
            _fieldInfos = new[]{
                new FieldInfo{
                    name = "id",
                    neverWrite = true,
                    componentType = "uint",
                    getCallback = () => _gridSettings.Key
                },
                new FieldInfo{
                    name = "name",
                    componentType = "string",
                    defaultCheck = "emptyString",
                    getCallback = () => _gridSettings.Name,
                    setCallback = v => _gridSettings.Name = (string)v
                },
                new FieldInfo{
                    name = "form_factor",
                    componentType = "string",
                    defaultCheck = "emptyString",
                    getCallback = () => _gridSettings.FormFactor,
                    setCallback = v => _gridSettings.FormFactor = (string)v
                },
                new FieldInfo{
                    name = "chunk_size",
                    componentType = "Vector3Int",
                    defaultCheck = "positiveV3Int",
                    getCallback = () => _gridSettings.ChunkSize,
                    setCallback = v => _gridSettings.ChunkSize = (Vector3Int)v
                },
                new FieldInfo{
                    name = "cell_size",
                    componentType = "Vector3",
                    defaultCheck = "positiveV3",
                    getCallback = () => _gridSettings.CellSize,
                    setCallback = v => _gridSettings.CellSize = (Vector3)v
                },
                new FieldInfo{
                    name = "has_bounds",
                    componentType = "bool",
                    getCallback = () => _gridSettings.Settings.hasBounds,
                    neverWrite = true
                },
                new FieldInfo{
                    name = "bounds",
                    componentType = "GridBounds",
                    defaultCheck = "bounds",
                    getCallback = () => _gridSettings.Settings.bounds,
                    neverWrite = true
                },
                new FieldInfo{
                    name = "layers",
                    componentType = "List",
                    getCallback = () => _gridSettings.Settings.layers,
                    neverWrite = true
                }
            };
        }

        private void OnChanged()
        {
            InvokeChangedFields();
        }

        private void InvokeChangedFields()
        {
            GridSettingsInfo currentInfo = default;
            if (_gridSettings != null) {
                currentInfo = _gridSettings.Info;
            }
            if (changed != null) {
                if (currentInfo.id != _prevValue.id) {
                    changed.Invoke("id");
                }
                if (currentInfo.content.name != _prevValue.content.name) {
                    changed.Invoke("name");
                }
                if (currentInfo.content.formFactor != _prevValue.content.formFactor) {
                    changed.Invoke("form_factor");
                }
                if (currentInfo.content.chunkSize != _prevValue.content.chunkSize) {
                    changed.Invoke("chunk_size");
                }
                if (currentInfo.content.cellSize != _prevValue.content.cellSize) {
                    changed.Invoke("cell_size");
                }
                if (currentInfo.content.hasBounds != _prevValue.content.hasBounds) {
                    changed.Invoke("has_bounds");
                }
                if (!currentInfo.content.bounds.Equals(_prevValue.content.bounds)) {
                    changed.Invoke("bounds");
                }
                if (currentInfo.content.layers != null || _prevValue.content.layers != null) {
                    if (currentInfo.content.layers == null || _prevValue.content.layers == null) {
                        changed.Invoke("layers");
                    } else if (currentInfo.content.layers.Count != _prevValue.content.layers.Count) {
                        changed.Invoke("layers");
                    } else {
                        for (int i = 0; i < currentInfo.content.layers.Count; i++) {
                            if (!currentInfo.content.layers[i].Equals(_prevValue.content.layers[i])) {
                                changed.Invoke("layers");
                                break;
                            }
                        }
                    }

                }
            }
        }
    }

    public abstract class DynamicWrapperBase : IDynamicWrapper
    {
        protected delegate string CheckComponentValueDelegate(object value);
        protected delegate object GetComponentDelegate();
        protected delegate void SetComponentDelegate(object value);
        protected struct FieldInfo
        {
            public string name;
            public string componentType;
            public string defaultCheck;
            public CheckComponentValueDelegate checkCallback;
            public GetComponentDelegate getCallback;
            public SetComponentDelegate setCallback;
            public bool neverRead;
            public bool neverWrite;
        }

        public Action<string> changed { get; set; }

        public Action locked { get; set; }

        public virtual bool IsLocked => false;

        protected FieldInfo[] _fieldInfos;
        private Dictionary<string, object> _cachedModelValues = new();

        public string CheckComponentValue(string name, object value)
        {
            var fieldInfo = _fieldInfos.Single(x => x.name == name);
            if (fieldInfo.checkCallback != null) {
                return fieldInfo.checkCallback(value);
            }

            switch (fieldInfo.defaultCheck) {
                case "emptyString":
                    if (string.IsNullOrWhiteSpace((string)value)) {
                        return $"Empty value of {name}";
                    }
                    break;

                case "positiveV3":
                    Vector3 v3 = (Vector3)value;
                    if (v3.x <= 0 || v3.y <= 0 || v3.z <= 0) {
                        return $"Components of {name} must be positive";
                    }
                    break;

                case "positiveV3Int":
                    Vector3Int v3i = (Vector3Int)value;
                    if (v3i.x <= 0 || v3i.y <= 0 || v3i.z <= 0) {
                        return $"Components of {name} must be positive";
                    }
                    break;

                case "bounds":
                    var bounds = (GridBoundsRect)value;
                    if (bounds.chunkFrom.x > bounds.chunkTo.x
                        || bounds.chunkFrom.y > bounds.chunkTo.y
                        || bounds.chunkFrom.z > bounds.chunkTo.z) {
                        return $"Bounds components of 'from' must by less or equal same components of 'to' ({name}:{bounds})";
                    }
                    break;
            }

            return null;
        }

        public IEnumerable<string> GetComponents()
        {
            return _fieldInfos.Select(x => x.name);
        }

        public string GetComponentType(string name)
        {
            return _fieldInfos.Single(x => x.name == name).componentType;
        }

        public object GetComponentValue(string name)
        {
            var fieldInfo = _fieldInfos.Single(x => x.name == name);
            if (fieldInfo.getCallback == null) {
                throw new Exception($"Field {name} don't support get method");
            }
            return fieldInfo.getCallback();
        }

        public DynamicFieldRights GetUserRights(string userId, string name)
        {
            //TODO implement users rights
            var fieldInfo = _fieldInfos.Single(x => x.name == name);
            return new DynamicFieldRights {
                read = !fieldInfo.neverRead,
                write = !fieldInfo.neverWrite
            };
        }

        public void SetComponentValue(string name, object value)
        {
            var fieldInfo = _fieldInfos.Single(x => x.name == name);
            if (fieldInfo.setCallback == null) {
                throw new Exception($"Field {name} don't support set method");
            }
            fieldInfo.setCallback(value);
        }

        public virtual bool TryLock()
        {
            return true;
            //TODO add object lock
        }

        public virtual void Unlock()
        {
            //TODO add object unlock
        }
    }

    public class DataLayerSettingsDynamicWrapper : DynamicWrapperBase
    {
        public DataLayerSettings DataLayerSettings {
            get => _dataLayerSettings;
            set {
                _dataLayerSettings = value;
            }
        }

        private DataLayerSettings _dataLayerSettings;

        public DataLayerSettingsDynamicWrapper()
        {
            _fieldInfos = new[]{
                new FieldInfo(){
                    name = "layer_type",
                    componentType = "enum",
                    checkCallback = OnLayerTypeCheck,
                    getCallback = () =>_dataLayerSettings.layerType,
                    setCallback = v
                        =>_dataLayerSettings.layerType=(LayerType)Enum.Parse(typeof(LayerType), (string)v)
                },
                new FieldInfo(){
                    name="tag",
                    componentType = "string",
                    defaultCheck = "emptyString",
                    getCallback = ()=>_dataLayerSettings.tag,
                    setCallback = v=>_dataLayerSettings.tag = (string)v
                },
                new FieldInfo(){
                    name = "chunk_size",
                    componentType = "Vector3Int",
                    defaultCheck = "positiveV3Int",
                    getCallback = ()=>_dataLayerSettings.chunkSize,
                    setCallback = v=>_dataLayerSettings.chunkSize=(Vector3Int)v
                },
                new FieldInfo(){
                    name = "has_view_layer",
                    componentType = "bool",
                    getCallback = ()=>_dataLayerSettings.hasViewLayer,
                    setCallback = v=>_dataLayerSettings.hasViewLayer =(bool)v
                }
            };
        }

        private string OnLayerTypeCheck(object value)
        {
            string str = (string)value;
            try {
                Enum.Parse(typeof(LayerType), str);
            } catch {
                return $"Value {str} no support";
            }
            return null;
        }

    }
}