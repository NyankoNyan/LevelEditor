﻿using Level.API;
using System;
using UnityEngine;

namespace Level.Builder
{
    //TODO remove file
    //public class LayerBuilder : MonoBehaviour, IBuilderCheck
    //{
    //    [SerializeField] private DataLayerSettings dataLayerSettings;
    //    [SerializeField] private LayerDataBuilder layerDataBuilder;

    //    public void Check()
    //    {
    //        if (!transform.parent && !transform.parent.GetComponent<ChunkBuilder>()) {
    //            Debug.LogError( $"{this}: Required {nameof( ChunkBuilder )} as parent" );
    //        }

    //        if (!layerDataBuilder) {
    //            Debug.LogError( $"{this}: Missing {nameof( layerDataBuilder )}" );
    //        }
    //    }

    //    public void Export(GridState gridState, Vector3Int chunkCoord, BlockProtoCollection blockProtos)
    //    {
    //        var chunk = gridState.GetChunk( chunkCoord );
    //        DataLayer layer = chunk.GetLayer( dataLayerSettings.layerType, dataLayerSettings.tag );
    //        layerDataBuilder.Export( layer, gridState, blockProtos );
    //    }

    //    public void Import(
    //        GridState gridState,
    //        Vector3Int chunkCoord,
    //        BlockProtoCollection blockProtos,
    //        LayerType layerType,
    //        string layerTag)
    //    {
    //        dataLayerSettings.layerType = layerType;
    //        dataLayerSettings.tag = layerTag;

    //        switch (layerType) {
    //            case LayerType.BlockLayer:
    //                layerDataBuilder = gameObject.AddComponent<BlockLayerDataBuilder>();
    //                break;

    //            default:
    //                throw new ArgumentException();
    //        }

    //        GridChunk chunk = gridState.GetChunk( chunkCoord );
    //        DataLayer layer = chunk.GetLayer( layerType, layerTag );
    //        layerDataBuilder.Import( layer, gridState, blockProtos );
    //    }
    //}
}