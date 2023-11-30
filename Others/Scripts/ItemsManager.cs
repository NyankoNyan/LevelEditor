using Unity.Netcode;

using UnityEngine;
using Level;
using LevelView;

namespace Items
{
    /// <summary>
    /// Просто менеджер для предметиков
    /// </summary>
    public class ItemsManager : MonoBehaviour
    {
        [SerializeField]
        bool _useNetwork;

        ItemsManagerNetwork _network;
        LevelStorage _levelStorage;

        private void Awake()
        {
            _network = GetComponent<ItemsManagerNetwork>();
        }

        private void Start(){
            _levelStorage = LevelStorage.Instance;            
        }

        #region API 

        public void CreateItem()
        {

        }

        public void OnItemCreated()
        {

        }

        public void AttachItem()
        {

        }

        public void DetachItem()
        {

        }

        public void TakeItem()
        {

        }

        public void DropItem()
        {

        }

        #endregion
    }

    public class ItemsManagerNetwork : NetworkBehaviour
    {

    }
}