using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level.API
{
    public class GridSettingsCollection:IEnumerable<GridSettings> {

        private GridSettingsRegistry _gridSettingsRegistry;
        private GridSettingsFabric _gridSettingsFabric;

        public Action<GridSettings> added;

        public Action<GridSettings> removed;

        public GridSettings Add(GridSettingsCreateParams? createParams = null, uint? id = null){
            
            const int nameTryCount = 10;
            GridSettingsCreateParams coolCreateParams;

            if (createParams==null){
                // Default create parameters
                coolCreateParams = new() {
                    chunkSize = Vector3Int.one,
                    formFactor = "block1x1x1",
                    hasBounds = false,
                    cellSize = Vector3.one,
                    layers = { new() {
                        layerType = LayerType.BlockLayer,
                        tag = "block"
                    } }
                };

                // Default name
                string baseName = $"grid_{_gridSettingsFabric.Counter + 1}";

                // Trying set unoccupied name
                bool nameOk = false;
                for (int i = 0; i < nameTryCount; i++) {
                    if(i==0){
                        coolCreateParams.name = baseName;
                    }else{
                        coolCreateParams.name = baseName + UnityEngine.Random.Range( 0, 10000 );
                    }
                    if (! _gridSettingsRegistry.Values.Any( x => x.Name == coolCreateParams.name )){
                        nameOk = true;
                        break;
                    }
                }
                if(!nameOk){
                    throw new LevelAPIException( $"Cant't find free name for new block proto" );
                }
            }else{
                coolCreateParams = createParams.Value;
            }

            if (string.IsNullOrWhiteSpace( gridSettings.name )) {
                throw new LevelAPIException( $"Empty grid settings name" );
            }
            if (_gridSettingsRegistry.Values.Any( x => x.Name == gridSettings.name )) {
                throw new LevelAPIException( $"Grid settings with name {gridSettings.name} already exists" );
            }

            if (id == null){
                return _gridSettingsFabric.Create( gridSettings ).Key;
            }else{
                if (id == 0){
                    throw new LevelAPIException( $"Empty grid settings id" );
                }
                return _gridSettingsFabric.CreateWithCounter( gridSettings, id );
            }            
        }

        public void Remove(uint id){
            GridSettings gridSettings = null;
            try {
                gridSettings = _gridSettingsRegistry.Values.First( x => x.Key == gridSettingsId );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing grid with id {gridSettingsId}" );
            }
        }

        public GridSettings this[uint id]{
            get {
                return _gridSettingsRegistry.Dict[gridSettingsId];
            }
        }

        public IEnumerator<GridSettings> GetEnumerator(){
            return _gridSettingsRegistry.Values.GetEnumerator();
        }

        private IEnumerator<GridSettings> GetEnumerator1(){
            return this.GetEnumerator
        } 

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator1();
        }
    }
}