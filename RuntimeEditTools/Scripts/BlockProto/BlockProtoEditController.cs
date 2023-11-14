using Level.API;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    internal class BlockProtoEditController : BaseParameterEditController
    {
        private BlockProtoCollection _blockProtoCollection;

        public BlockProtoEditController(
            BlockProtoCollection blockProtoAPI,
            ParametersListFacade facade,
            Keyboard keyboard)
            : base( facade, keyboard )
        {
            Assert.IsNotNull( blockProtoAPI );

            _blockProtoCollection = blockProtoAPI;
            _connector = new BlockProtoParametersConnector( _blockProtoCollection );
        }

        public void SetBlockId(uint blockId)
        {
            ( _connector as BlockProtoParametersConnector ).BlockId = blockId;
        }
    }
}