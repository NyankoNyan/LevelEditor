using UnityEngine;
using Unity.Netcode;
using LevelView;

namespace Level.API
{
    /// <summary>
    /// Интерфейс нужен для возможности выкинуть Netcode из проекта
    /// </summary>
    public interface ILevelSyncronizer
    {

    }

    public delegate void BlockProtoRequestCallback(IEnumerable<BlockProtoInfo> blockProtos);
    public delegate void GridSettingsRequestCallback(IEnumerable<GridSettingsInfo> gridSettings);
    public delegate void GridStateRequestCallback(IEnumerable<GridStateInfo> gridStates);

    public class NetcodeLevelSyncronizer : NetworkBehaviour, ILevelSyncronizer
    {

        private LevelStorage _levelStorage;

        void Awake()
        {
            _levelStorage = LevelStorage.Instance;
            if (_levelStorage == null) {
                throw new Exception();
            }
        }

        #region Blocks
        public void GetAllBlockProtoRequest(BlockProtoRequestCallback callback)
        {

        }

        [ServerRPC]
        private void GetAllBlockProtoServerRPC()
        {

        }

        [ClientRPC]
        private void GetAllBlockProtoClientRPC(){

        }

        public void BlockProtoChangesSend(
            IEnumerable<BlockProto> changed, 
            IEnumerable<BlockProto> added, 
            IEnumerable<uint> removed){

        }

        [ClientRPC]
        private void BlockProtoChangesClientRPC(){

        }

        public void BlockProtoModifyRequest(){

        }

        [ServerRPC]
        private void BlockProtoModifyServerRPC(){

        }
        #endregion

        #region GridSettings
        public void GridSettingsRequest(){

        }
        #endregion

    }
}