﻿using System;
using UnityEngine;
using System.Collections.Generic;
using Level.API;
using UnityEngine.Assertions;

namespace Level.Builder
{
    public class BlockLayerDataBuilder : LayerDataBuilder
    {
        [SerializeField] List<Block> blocks = new();

        public override void Export(
            DataLayer dataLayer,
            GridState gridStateAPI,
            IBlockProtoAPI blockProtoAPI)
        {
            var blockLayer = dataLayer as BlockLayer;
            Assert.IsNotNull( blockLayer );

            BlockData[] data = blockLayer.Data;

            HashSet<Vector3Int> coords = new();

            Vector3Int chunkSize = gridStateAPI.GridSettings.ChunkSize;
            for (int i = 0; i < blocks.Count; i++) {
                Block block = blocks[i];
                if (coords.Contains( block.coord )) {
                    Debug.LogError( $"Duplicate coordinate {block.coord} on index {i}", gameObject );
                } else {
                    coords.Add( block.coord );
                    int blockIndex;
                    try {
                        blockIndex = GridChunk.BlockCoordToFlatSafe( block.coord, chunkSize );
                    } catch (Exception e) {
                        Debug.LogError( $"{e.Message} on index {i}", gameObject );
                        continue;
                    }
                    var blockProto = blockProtoAPI.GetBlockProto( block.id );
                    data[blockIndex].blockId = (ushort)blockProto.Key; //TODO make same type
                }
            }
        }

        public override void Import(
            DataLayer dataLayer,
            GridState gridStateAPI,
            IBlockProtoAPI blockProtoAPI)
        {
            blocks.Clear();
            var blockLayer = dataLayer as BlockLayer;
            Assert.IsNotNull( blockLayer );

            Vector3Int chunkSize = gridStateAPI.GridSettings.ChunkSize;

            for (int i = 0; i < blockLayer.Data.Length; i++) {
                BlockData bd = blockLayer.Data[i];
                if (bd.blockId > 0) {
                    Block block = new Block();
                    block.coord = GridChunk.FlatToBlockCoordSafe( i, chunkSize );
                    var blockProto = blockProtoAPI.GetBlockProto( bd.blockId );
                    block.id = blockProto.Name;
                    blocks.Add( block );
                }
            }
        }

        [Serializable]
        public struct Block
        {
            public string id;
            public Vector3Int coord;
        }

        public static BlockLayerDataBuilder Create(string tag, Transform parent)
        {
            var go = new GameObject( tag );
            go.transform.parent = parent;
            return go.AddComponent<BlockLayerDataBuilder>();
        }
    }
}
