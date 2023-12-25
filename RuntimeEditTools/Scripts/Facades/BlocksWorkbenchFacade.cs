using Level;
using LevelView;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeEditTools
{
    public class BlocksWorkbenchFacade : MonoBehaviour
    {
        [SerializeField] private BlockEditorFacade _blockEditorFacade;
        [SerializeField] private ListField _blocksListFacade;

        private Action _destroyCallback;

        public void Init()
        {
            var blockCollection = LevelStorage.Instance.API.BlockProtoCollection;

            IListFacade blocksList = _blocksListFacade;

            // Связываем список прототипов блоков
            // Этот блок наглядно демонстрирует, почему нужно использовать библиотеки реактивного связывания
            Dictionary<BlockProto, ITextConnector> blockTexts = new();
            Dictionary<BlockProto, Action> blockSubscriptions = new();
            Action<BlockProto> onBlockProtoAdded = (block) => {
                var connector = blocksList.AddElement( block.Name );
                blockTexts.Add( block, connector );
            };
            blockCollection.added += onBlockProtoAdded;
            foreach (var block in blockCollection) {
                onBlockProtoAdded( block );
                Action onBlockDestroy = null;
                onBlockDestroy = () => {
                    block.OnDestroyAction -= onBlockDestroy;
                    blockTexts[block].Remove();
                    blockTexts.Remove( block );
                    blockSubscriptions.Remove( block );
                };
                block.OnDestroyAction += onBlockDestroy;
                blockSubscriptions.Add( block, onBlockDestroy );
            }

            Action onDestroy = null;
            onDestroy = () => {
                _destroyCallback -= onDestroy;
                blockCollection.added -= onBlockProtoAdded;
                foreach (var sub in blockSubscriptions) {
                    sub.Key.OnDestroyAction -= sub.Value;
                }
            };
            _destroyCallback += onDestroy;
        }

        private void OnDestroy()
        {
            _destroyCallback?.Invoke();
        }
    }
}