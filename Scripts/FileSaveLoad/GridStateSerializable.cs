using Level.API;
using System;

namespace Level.IO
{
    [Serializable]
    internal class GridStateSerializable
    {
        public uint id;
        public uint gridSettingsId;

        public static explicit operator GridStateSerializable(GridState grid)
            => new() {
                id = grid.Key,
                gridSettingsId = grid.GridSettings.Key
            };

        public void Load(GridStatesCollection gridStatesCollection, GridSettingsCollection gridSettingsCollection)
        {
            gridStatesCollection.Add( gridSettingsId, id );
        }
    }
}