using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level.API
{
    /// <summary>
    /// Коллекция для хранения описаний блоков (без графония).
    /// Инстанции блоков хранятся в другом месте.
    /// </summary>
    public class BlockProtoCollection : IEnumerable<BlockProto>
    {
        public Action<BlockProto> added;
        public Action<BlockProto> removed;

        private readonly BlockProtoRegistry _blockProtoRegistry;
        private readonly BlockProtoFabric _blockProtoFabric;

        public BlockProtoCollection()
        {
            _blockProtoRegistry = new();
            _blockProtoFabric = new();

            _blockProtoFabric.onCreate += OnCreateFab;
            _blockProtoRegistry.onAdd += OnAddReg;
            _blockProtoRegistry.onRemove += OnRemoveReg;
        }

        public void Destroy()
        {
            _blockProtoFabric.onCreate -= OnCreateFab;
            _blockProtoRegistry.onAdd -= OnAddReg;
            _blockProtoRegistry.onRemove -= OnRemoveReg;
        }

        /// <summary>
        /// Добавляет описание блока в коллекцию
        /// </summary>
        /// <param name="blockProtoSettings"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="LevelAPIException"></exception>
        public BlockProto Add(BlockProtoSettings? blockProtoSettings = null, uint? id = null)
        {
            BlockProtoSettings localSettings;
            const int nameTryCount = 10;

            if (blockProtoSettings.HasValue) {
                localSettings = blockProtoSettings.Value;

                if (_blockProtoRegistry.Values.Any( x => x.Name == localSettings.name )) {
                    throw new LevelAPIException( $"Block proto with name {localSettings.name} already exists" );
                }
            } else {
                bool nameOk = false;
                string baseName = $"block_{_blockProtoFabric.Counter + 1}";
                localSettings = new BlockProtoSettings() {
                    name = baseName,
                    size = Vector3Int.one
                };
                // TODO external method for names
                for (int i = 0; i < nameTryCount; i++) {
                    if (i == 0) {
                        localSettings.name = baseName;
                    } else {
                        localSettings.name = baseName + UnityEngine.Random.Range( 0, 10000 );
                    }
                    if (_blockProtoRegistry.Values.All( x => x.Name != localSettings.name )) {
                        nameOk = true;
                        break;
                    }
                }
                if (!nameOk) {
                    throw new LevelAPIException( $"Cant't find free name for new block proto" );
                }
            }

            var createParams = new BlockProtoCreateParams( localSettings );
            if (id.HasValue) {
                return _blockProtoFabric.CreateWithCounter( createParams, id.Value );
            } else {
                return _blockProtoFabric.Create( createParams );
            }
        }

        public void Remove(uint id)
        {
            BlockProto blockProto = null;
            try {
                blockProto = _blockProtoRegistry.Values.First( x => x.Key == id );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing block with id {id}" );
            }
            // TODO Check block proto references
            blockProto.Destroy();
        }

        public BlockProto this[uint id]
        {
            get {
                return _blockProtoRegistry.Dict[id];
            }
        }

        public BlockProto FindByName(string name)
        {
            return _blockProtoRegistry.Values.SingleOrDefault( x => x.Name == name );
        }

        public IEnumerator<BlockProto> GetEnumerator() => _blockProtoRegistry.Values.GetEnumerator();

        private IEnumerator<BlockProto> GetEnumerator1() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();

        private void OnCreateFab(BlockProto blockProto) => _blockProtoRegistry.Add(blockProto);
        private void OnAddReg(BlockProto blockProto) => added?.Invoke(blockProto);
        private void OnRemoveReg(BlockProto blockProto) => removed?.Invoke(blockProto);
    }
}