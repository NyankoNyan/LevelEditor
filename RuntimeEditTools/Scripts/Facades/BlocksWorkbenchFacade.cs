using Level;
using LevelView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public class BlocksWorkbenchFacade : MonoBehaviour
    {
        [SerializeField] BlockEditorFacade _blockEditorFacade;
        [SerializeField] ListField _blocksListFacade;

        private UnityAction _destroyCallback;

        public void Init()
        {
            var blocksAPI = LevelStorage.Instance.API.BlockProto;

            IListFacade blocksList = _blocksListFacade;

            // Связываем список прототипов блоков
            // Этот блок наглядно демонстрирует, почему нужно использовать библиотеки реактивного связывания
            Dictionary<BlockProto, ITextConnector> blockTexts = new();
            Dictionary<BlockProto, UnityAction> blockSubscriptions = new();
            UnityAction<BlockProto> onBlockProtoAdded = (block) => {
                var connector = blocksList.AddElement( block.Name );
                blockTexts.Add( block, connector );
            };
            blocksAPI.onBlockProtoAdded += onBlockProtoAdded;
            foreach (var block in blocksAPI.BlockProtos) {
                onBlockProtoAdded( block );
                UnityAction onBlockDestroy = null;
                onBlockDestroy = () => {
                    block.OnDestroyAction -= onBlockDestroy;
                    blockTexts[block].Remove();
                    blockTexts.Remove( block );
                    blockSubscriptions.Remove( block );
                };
                block.OnDestroyAction += onBlockDestroy;
                blockSubscriptions.Add( block, onBlockDestroy );
            }


            UnityAction onDestroy = null;
            onDestroy = () => {
                _destroyCallback -= onDestroy;
                blocksAPI.onBlockProtoAdded -= onBlockProtoAdded;
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
