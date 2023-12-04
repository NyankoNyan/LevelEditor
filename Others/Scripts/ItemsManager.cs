using LevelView;

using Unity.Netcode;

using UnityEngine;

namespace Items
{
    /// <summary>
    /// Просто менеджер для предметиков
    /// </summary>
    public class ItemsManager : MonoBehaviour
    {
        [SerializeField]
        private bool _useNetwork;

        private ItemsManagerNetwork _network;
        private LevelStorage _levelStorage;

        private void Awake()
        {
            _network = GetComponent<ItemsManagerNetwork>();
        }

        private void Start()
        {
            _levelStorage = LevelStorage.Instance;
        }

        #region API

        public GameObject CreateItem(string name)
        {
            GameObject go = new(name);
            return go;
        }

        public void RemoveItem(GameObject item)
        {
        }

        public void AttachItem(GameObject item, GameObject targetItem)
        {
            item.transform.parent = targetItem.transform;
        }

        public void DetachItem(GameObject item)
        {
            item.transform.parent = null;
        }

        public void TakeItem(GameObject item, GameObject hand)
        {
            item.transform.parent = hand.transform;
        }

        public void DropItem(GameObject item)
        {
            if (_useNetwork) {
                var no = item.GetComponent<NetworkObject>();
                _network.DropItemRequest(no.NetworkObjectId);
            } else {
                item.transform.parent = null;
            }
        }

        #endregion API
    }

    public class ItemsManagerNetwork : NetworkBehaviour
    {
        public void DropItemRequest(ulong netId)
        {
        }

        [ServerRpc]
        public void DropItemRequestServerRpc()
        {
        }

        [ClientRpc]
        public void DropItemResponceClientRpc()
        {
        }
    }
}