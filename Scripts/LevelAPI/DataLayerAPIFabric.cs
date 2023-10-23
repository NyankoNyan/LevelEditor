using System;

namespace Level.API
{

    public interface IDataLayerAPI
    {
        LayerType LayerType { get; }
    }

    internal class DataLayerAPIFabric
    {
        public IDataLayerAPI Create(DataLayerSettings dataLayerSettings, GridState gridState)
        {
            if (dataLayerSettings.layerType == LayerType.BlockLayer) {
                return new BlockLayerAPI( gridState, dataLayerSettings.tag );
            } else {
                throw new ArgumentException();
            }
        }
    }

}
