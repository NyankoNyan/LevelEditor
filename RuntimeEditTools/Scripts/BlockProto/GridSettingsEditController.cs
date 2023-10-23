using Level.API;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    internal class GridSettingsEditController : BaseParameterEditController
    {
        private IGridSettingsAPI _gridSettingsAPI;

        public GridSettingsEditController(
            IGridSettingsAPI gridSettingsAPI,
            ParametersListFacade facade,
            Keyboard keyboard)
            : base( facade, keyboard )
        {
            Assert.IsNotNull( gridSettingsAPI );

            _gridSettingsAPI = gridSettingsAPI;
            _connector = new GridSettingsParametersConnector( _gridSettingsAPI );
        }

        public void SetGridSettingsId(uint gridSettingsId)
        {
            ( _connector as GridSettingsParametersConnector ).GridSettingsId = gridSettingsId;
        }
    }
}