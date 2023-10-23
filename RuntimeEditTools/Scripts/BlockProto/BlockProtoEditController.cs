using Level.API;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    internal class BlockProtoEditController : BaseParameterEditController
    {
        private IBlockProtoAPI _blockProtoAPI;

        public BlockProtoEditController(
            IBlockProtoAPI blockProtoAPI,
            ParametersListFacade facade,
            Keyboard keyboard)
            : base( facade, keyboard )
        {
            Assert.IsNotNull( blockProtoAPI );

            _blockProtoAPI = blockProtoAPI;
            _connector = new BlockProtoParametersConnector( _blockProtoAPI );
        }

        public void SetBlockId(uint blockId)
        {
            ( _connector as BlockProtoParametersConnector ).BlockId = blockId;
        }
    }
}