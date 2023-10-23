using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Level.API
{
    public interface IBlockProtoAPI
    {
        UnityAction<BlockProto> onBlockProtoAdded { get; set; }
        UnityAction<BlockProto> onBlockProtoRemoved { get; set; }
        uint Add(BlockProtoSettings blockProtoSettings);
        uint AddEmpty();
        BlockProto Add(BlockProtoSettings blockProtoSettings, uint id);
        void Remove(uint blockProtoId);
        IEnumerable<BlockProto> BlockProtos { get; }
        BlockProto GetBlockProto(string blockProtoName);
        BlockProto GetBlockProto(uint id);
    }


    public class BlockProtoAPI : IBlockProtoAPI
    {
        private BlockProtoRegistry _blockProtoRegistry;
        private BlockProtoFabric _blockProtoFabric;

        public IEnumerable<BlockProto> BlockProtos
            => _blockProtoRegistry.Values;

        public UnityAction<BlockProto> onBlockProtoAdded { get => _blockProtoFabric.onCreate; set => _blockProtoFabric.onCreate = value; }

        public UnityAction<BlockProto> onBlockProtoRemoved { get => _blockProtoRegistry.onRemove; set => _blockProtoRegistry.onRemove = value; }

        internal BlockProtoAPI(BlockProtoRegistry blockProtoRegistry, BlockProtoFabric blockProtoFabric)
        {
            _blockProtoRegistry = blockProtoRegistry;
            _blockProtoFabric = blockProtoFabric;
        }

        public uint Add(BlockProtoSettings blockProtoSettings)
        {
            if (_blockProtoRegistry.Values.Any( x => x.Name == blockProtoSettings.name )) {
                throw new LevelAPIException( $"Block proto with name {blockProtoSettings.name} already exists" );
            }
            var createParams = new BlockProtoCreateParams( blockProtoSettings );
            return _blockProtoFabric.Create( createParams ).Key;
        }

        public BlockProto Add(BlockProtoSettings blockProtoSettings, uint id)
        {
            if (_blockProtoRegistry.Values.Any( x => x.Name == blockProtoSettings.name )) {
                throw new LevelAPIException( $"Block proto with name {blockProtoSettings.name} already exists" );
            }
            var createParams = new BlockProtoCreateParams( blockProtoSettings );
            return _blockProtoFabric.CreateWithCounter( createParams, id );
        }

        public void Remove(uint blockProtoId)
        {
            BlockProto blockProto = null;
            try {
                blockProto = _blockProtoRegistry.Values.First( x => x.Key == blockProtoId );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing block with id {blockProtoId}" );
            }
            blockProto.Destroy();
        }

        public BlockProto GetBlockProto(string blockProtoName)
            => _blockProtoRegistry.Values.Single( x => x.Name == blockProtoName );

        public BlockProto GetBlockProto(uint id)
            => _blockProtoRegistry.Dict[id];

        public uint AddEmpty()
        {
            const int nameTryCount = 10;
            string baseName = $"block_{_blockProtoFabric.Counter + 1}";
            try {
                var blockProtoSettings = new BlockProtoSettings() {
                    name = baseName
                };
                return Add( blockProtoSettings );
            } catch (LevelAPIException) {
                for (int i = 0; i < nameTryCount; i++) {
                    try {
                        var blockProtoSettings = new BlockProtoSettings() {
                            name = baseName + UnityEngine.Random.Range( 0, 10000 ),
                            size = Vector3Int.one
                        };
                        return Add( blockProtoSettings );
                    } catch (LevelAPIException) { }
                }
                throw new LevelAPIException( $"Cant't find free name for new block proto" );
            }
        }

        //private void SubscribeToBlockDestroy(BlockProto blockProto)
        //{
        //    UnityAction onDestroyAction = null;
        //    onDestroyAction = () => {
        //        blockProto.OnDestroyAction -= onDestroyAction;
        //        _onBlockProtoRemoved?.Invoke( blockProto );
        //    };
        //    blockProto.OnDestroyAction += onDestroyAction;
        //}
    }
}
