using Level.API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public class BlockProtoParametersConnector : IParametersConnector
    {
        private IBlockProtoAPI _blockProtoAPI;

        private const string ID = "Id";
        private const string NAME = "Name";
        private const string FORMFACTOR = "FormFactor";
        private const string LAYERTAG = "LayerTag";
        private const string SIZE = "Size";

        private static readonly ParameterDescription[] parametersDescriptions = {
            new(ID, typeof(uint), ParameterMode.ReadOnly),
            new(NAME, typeof(string), ParameterMode.ReadWrite),
            new(FORMFACTOR, typeof(string), ParameterMode.ReadWrite),
            new(LAYERTAG, typeof(string), ParameterMode.ReadWrite),
            new(SIZE, typeof(Vector3Int), ParameterMode.ReadWrite)
        };

        private uint _blockId;

        private UnityAction<string, object> _onParamModelUpdate;

        public uint BlockId
        {
            get => _blockId;
            set {
                if (_blockId != 0) {
                    ReleaseAPIReceivers();
                }
                _blockId = value;
                if (_blockId != 0) {
                    SetupAPIReceivers();
                    UpdateAll();
                }
            }
        }

        public UnityAction<string, object> OnParamModelUpdate
        {
            get => _onParamModelUpdate;
            set => _onParamModelUpdate = value;
        }

        public BlockProtoParametersConnector(IBlockProtoAPI blockProtoAPI)
        {
            _blockProtoAPI = blockProtoAPI;
        }

        public IEnumerable<ParameterDescription> ParametersDescriptions => parametersDescriptions;

        public object GetParameter(string name)
        {
            var blockProto = _blockProtoAPI.GetBlockProto( BlockId );
            switch (name) {
                case ID:
                    return blockProto.Key;

                case NAME:
                    return blockProto.Name;

                case FORMFACTOR:
                    return blockProto.FormFactor;

                case LAYERTAG:
                    return blockProto.Tag;

                case SIZE:
                    return blockProto.Size;

                default:
                    throw new ArgumentException( $"Unknown parameter {name}" );
            }
        }

        public void SetParameter(string name, object value)
        {
            var blockProto = _blockProtoAPI.GetBlockProto( BlockId );
            switch (name) {
                case NAME:
                    blockProto.Name = (string)value;
                    break;

                case FORMFACTOR:
                    blockProto.FormFactor = (string)value;
                    break;

                case LAYERTAG:
                    blockProto.Tag = (string)value;
                    break;

                case SIZE:
                    blockProto.Size = (Vector3Int)value;
                    break;
            }
        }

        private void UpdateAll()
        {
            foreach (var descr in parametersDescriptions) {
                _onParamModelUpdate?.Invoke( descr.name, GetParameter( descr.name ) );
            }
        }

        private void ReleaseAPIReceivers()
        {
            var blockProto = _blockProtoAPI.GetBlockProto( BlockId );
            blockProto.changed -= OnModelChanged;
        }

        private void SetupAPIReceivers()
        {
            var blockProto = _blockProtoAPI.GetBlockProto( BlockId );
            blockProto.changed += OnModelChanged;
        }

        private void OnModelChanged() => UpdateAll();
    }
}