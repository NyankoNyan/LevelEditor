using Level.API;
using UnityEngine.Assertions;

namespace RuntimeEditTools
{
    internal class GridSettingsEditController : BaseParameterEditController
    {
        private GridSettingsCollection _gridSettingsAPI;

        public GridSettingsEditController(
            GridSettingsCollection gridSettingsAPI,
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