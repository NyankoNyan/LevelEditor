using Level.API;
using System;

namespace Level.IO
{
    [Serializable]
    internal class GridSettingsSerializable
    {
        public uint id;
        public GridSettingsCreateParams settings;

        public static explicit operator GridSettingsSerializable(GridSettings gridSettings)
            => new() {
                id = gridSettings.Key,
                settings = gridSettings.Settings
            };

        public void Load(GridSettingsCollection gridSettingsCollection)
        {
            gridSettingsCollection.Add( settings, id );
        }
    }
}