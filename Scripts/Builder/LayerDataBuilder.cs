using Level.API;
using System;
using UnityEngine;

namespace Level.Builder
{
    public abstract class LayerDataBuilder : MonoBehaviour
    {
        public abstract void Export(
            DataLayer dataLayer,
            GridState gridStateAPI,
            IBlockProtoAPI blockProtoAPI);

        public abstract void Import(
            DataLayer dataLayer,
            GridState gridStateAPI,
            IBlockProtoAPI blockProtoAPI);
    }

    public class LayerDataBulderFabric
    {
        public LayerDataBuilder Create(LayerType layerType, string tag, Transform parent)
        {
            switch (layerType) {
                case LayerType.BlockLayer:
                    return BlockLayerDataBuilder.Create( tag, parent );
                default:
                    throw new ArgumentException();
            }
        }
    }
}
