using Level;
using Level.API;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    internal class GridSettingsParametersConnector : IParametersConnector
    {
        private const string ID = "Id";
        private const string NAME = "Name";
        private const string CHUNKSIZE = "Chunk size";
        private const string CELLSIZE = "Cell size";
        private const string FORMFACTOR = "Form factor";
        private const string HASBOUNDS = "Has bounds";
        private const string BOUNDS = "Bounds";

        private static readonly ParameterDescription[] parametersDescriptions = {
            new(ID, typeof(uint), ParameterMode.ReadOnly),
            new(NAME, typeof(string), ParameterMode.ReadWrite),
            new(FORMFACTOR, typeof(string), ParameterMode.ReadWrite),
            new(CHUNKSIZE, typeof(Vector3Int), ParameterMode.ReadWrite),
            new(CELLSIZE, typeof(Vector3), ParameterMode.ReadWrite),
            new(HASBOUNDS, typeof(bool), ParameterMode.ReadOnly),
            new(BOUNDS, typeof(GridBoundsRect), ParameterMode.ReadOnly)
        };

        private uint _gridSettingsId;
        private GridSettingsCollection _gridSettingsAPI;

        public UnityAction<string, object> OnParamModelUpdate { get; set; }

        public uint GridSettingsId
        {
            get => _gridSettingsId;
            set {
                if (_gridSettingsId != 0) {
                    ReleaseAPIReceivers();
                }
                _gridSettingsId = value;
                if (_gridSettingsId != 0) {
                    SetupAPIReceivers();
                    UpdateAll();
                }
            }
        }

        public GridSettingsParametersConnector(GridSettingsCollection gridSettingsAPI)
        {
            _gridSettingsAPI = gridSettingsAPI;
        }

        public object GetParameter(string name)
        {
            var gridSettings = _gridSettingsAPI[_gridSettingsId];
            switch (name) {
                case ID:
                    return gridSettings.Key;

                case NAME:
                    return gridSettings.Name;

                case CHUNKSIZE:
                    return gridSettings.ChunkSize;

                case CELLSIZE:
                    return gridSettings.CellSize;

                case FORMFACTOR:
                    return gridSettings.FormFactor;

                case HASBOUNDS:
                    return gridSettings.Settings.hasBounds;

                case BOUNDS:
                    return gridSettings.Settings.bounds;

                default:
                    throw new ArgumentException( $"Unknown parameter {name}" );
            }
        }

        public IEnumerable<ParameterDescription> ParametersDescriptions => parametersDescriptions;

        public void SetParameter(string name, object value)
        {
            var gridSettings = _gridSettingsAPI[_gridSettingsId];
            switch (name) {
                case NAME:
                    gridSettings.Name = (string)value;
                    break;

                case CHUNKSIZE:
                    gridSettings.ChunkSize = (Vector3Int)value;
                    break;

                case CELLSIZE:
                    gridSettings.CellSize = (Vector3)value;
                    break;

                case FORMFACTOR:
                    gridSettings.FormFactor = (string)value;
                    break;
            }
        }

        private void UpdateAll()
        {
            foreach (var descr in parametersDescriptions) {
                OnParamModelUpdate?.Invoke( descr.name, GetParameter( descr.name ) );
            }
        }

        private void ReleaseAPIReceivers()
        {
            var gridSettings = _gridSettingsAPI[_gridSettingsId];
            gridSettings.changed -= OnModelChanged;
        }

        private void SetupAPIReceivers()
        {
            var gridSettings = _gridSettingsAPI[_gridSettingsId];
            gridSettings.changed += OnModelChanged;
        }

        private void OnModelChanged() => UpdateAll();
    }
}