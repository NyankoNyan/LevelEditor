using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Level.API
{
    /// <summary>
    /// Коллекция для хранения описаний блоков (без графония). 
    /// Инстанции блоков хранятся в другом месте.
    /// </summary>
    public class BlockProtoCollection:IEnumerable<BlockProto>
    {
        // TODO Протянуть события
        public Action<BlockProto> added;
        public Action<BlockProto> removed;

        private BlockProtoRegistry _blockProtoRegistry;
        private BlockProtoFabric _blockProtoFabric;

        public BlockProtoCollection()
        {
            _blockProtoRegistry = new();
            _blockProtoFabric = new();

            _blockProtoFabric.onCreate += gs => _blockProtoRegistry.Add( gs );
        }

        public void Destroy()
        {
            //TODO dispose
        }

        /// <summary>
        /// Добавляет описание блока в коллекцию
        /// </summary>
        /// <param name="blockProtoSettings"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="LevelAPIException"></exception>
        public uint Add(BlockProtoSettings? blockProtoSettings = null, uint? id = null){
            BlockProtoSettings localSettings;
            const int nameTryCount = 10;

            if (blockProtoSettings.HasValue){
                localSettings = blockProtoSettings.Value;

                if (_blockProtoRegistry.Values.Any(x => x.Name == localSettings.name)){
                    throw new LevelAPIException($"Block proto with name {localSettings.name} already exists");
                }
            }else{
                bool nameOk = false;
                string baseName = $"block_{_blockProtoFabric.Counter + 1}";
                localSettings = new BlockProtoSettings(){
                    name = baseName,
                    sizeof = Vector3Int.one
                };
                // TODO external method for names
                for (int i = 0; i < nameTryCount; i++){
                    if (i == 0){
                        localSettings.name = baseName;
                    }else{
                        localSettings.name = baseName + UnityEngine.Random.Range(0, 10000),
                    }
                    if (_blockProtoRegistry.Values.All(x => x.Name != localSettings.name)){
                        nameOk = true;
                        break;
                    }
                }
                if (!nameOk) {
                    throw new LevelAPIException($"Cant't find free name for new block proto");
                }
            }

            var createParams = new BlockProtoCreateParams(localSettings);
            if (id.HasValue){
                return _blockProtoFabric.CreateWithCounter(createParams, id);
            }else{
                return _blockProtoFabric.Create(createParams).Key;
            }
        }

        public void Remove(uint id){
            BlockProto blockProto = null;
            try{
                blockProto = _blockProtoRegistry.Values.First(x => x.Key == id);
            } catch (InvalidOperationException){
                throw new LevelAPIException($"Missing block with id {id}");
            }
            // TODO Check block proto references
            blockProto.Destroy();
        }

        public BlockProto this[uint id]{
            get{
                return _blockProtoRegistry.Dict[id];
            }
        }

        public BlockProto FindByName(string name){
            return _blockProtoRegistry.Values.SingleOrDefault(x => x.Name == name);
        }

        public IEnumerator<BlockProto> GetEnumerator() => _blockProtoRegistry.Values.GetEnumerator();

        private IEnumerator<BlockProto> GetEnumerator1() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();
    }
}
