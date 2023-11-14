using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level.API
{
    /// <summary>
    /// Коллекция описаний пространственных сеток
    /// </summary>
    public class GridSettingsCollection : IEnumerable<GridSettings>
    {
        //TODO протянуть события
        public Action<GridSettings> added;

        public Action<GridSettings> removed;

        private GridSettingsRegistry _gridSettingsRegistry;
        private GridSettingsFabric _gridSettingsFabric;

        public GridSettingsCollection()
        {
            _gridSettingsRegistry = new();
            _gridSettingsFabric = new();
            _gridSettingsFabric.onCreate += gs => _gridSettingsRegistry.Add( gs );
        }

        public void Dispose()
        {
            //TODO dispose
        }

        /// <summary>
        /// Добавляет в коллекцию новые описания пространственных сеток.
        /// </summary>
        /// <param name="createParams">Настройки создания сетки. Если пустое, то создаст новую сетку с настройками по умолчанию.</param>
        /// <param name="id">Индекс добавляемого элемента (нужно для загрузки уровня). Опциональное поле.</param>
        /// <returns></returns>
        public GridSettings Add(GridSettingsCreateParams? createParams = null, uint? id = null)
        {
            const int nameTryCount = 10;
            GridSettingsCreateParams coolCreateParams;

            if (createParams == null) {
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
                    if (i == 0) {
                        coolCreateParams.name = baseName;
                    } else {
                        coolCreateParams.name = baseName + UnityEngine.Random.Range( 0, 10000 );
                    }
                    if (!_gridSettingsRegistry.Values.Any( x => x.Name == coolCreateParams.name )) {
                        nameOk = true;
                        break;
                    }
                }
                if (!nameOk) {
                    throw new LevelAPIException( $"Cant't find free name for new block proto" );
                }
            } else {
                coolCreateParams = createParams.Value;
            }

            if (string.IsNullOrWhiteSpace( coolCreateParams.name )) {
                throw new LevelAPIException( $"Empty grid settings name" );
            }
            if (_gridSettingsRegistry.Values.Any( x => x.Name == coolCreateParams.name )) {
                throw new LevelAPIException( $"Grid settings with name {coolCreateParams.name} already exists" );
            }

            if (id == null) {
                return _gridSettingsFabric.Create( coolCreateParams );
            } else {
                if (id.Value == 0) {
                    throw new LevelAPIException( $"Empty grid settings id" );
                }
                return _gridSettingsFabric.CreateWithCounter( coolCreateParams, id.Value );
            }
        }

        /// <summary>
        /// Удаляет описание с указанным индексом.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(uint id)
        {
            GridSettings gridSettings = null;
            //TODO проверкм наличия сеток с текущим описанием
            try {
                gridSettings = _gridSettingsRegistry.Values.First( x => x.Key == id );
            } catch (InvalidOperationException) {
                throw new LevelAPIException( $"Missing grid with id {id}" );
            }
            //TODO удалять всё же
        }

        /// <summary>
        /// Возвращает описание по индексу.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GridSettings this[uint id]
        {
            get {
                return _gridSettingsRegistry.Dict[id];
            }
        }

        /// <summary>
        /// Ищет описание по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Возвращает объект параметров грида. Если ничего не найдено, вернёт null.</returns>
        public GridSettings FindByName(string name)
        {
            return _gridSettingsRegistry.Values.SingleOrDefault( x => x.Name == name );
        }

        public IEnumerator<GridSettings> GetEnumerator() => _gridSettingsRegistry.Values.GetEnumerator();

        private IEnumerator<GridSettings> GetEnumerator1() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();
    }
}