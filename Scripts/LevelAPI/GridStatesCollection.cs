using System;
using System.Collections;
using System.Collections.Generic;

namespace Level.API
{
    public class GridStatesCollection : IEnumerable<GridState>
    {
        public Action<GridState> added;
        public Action<GridState> removed;

        private LevelAPI _level;
        private GridStateRegistry _gridStateRegistry;
        private GridStateFabric _gridStateFabric;
        private DataLayerFabric _dataLayerFabric;

        public LevelAPI Level => _level;

        public GridStatesCollection(LevelAPI level)
        {
            _level = level;

            _gridStateRegistry = new();
            _gridStateFabric = new();
            _dataLayerFabric = new();

            _gridStateFabric.onCreate += OnFacCreate;
            _gridStateRegistry.onAdd += OnRegAdd;
            _gridStateRegistry.onRemove += OnRegRemove;
        }

        public void Destroy()
        {
            _gridStateRegistry.onAdd -= OnRegAdd;
            _gridStateRegistry.onRemove -= OnRegRemove;
            _gridStateFabric.onCreate -= OnFacCreate;
        }

        /// <summary>
        /// Создаёт новую сетку на основе её описания.
        /// </summary>
        /// <param name="gridSettingsId">Индекс описания пространственных сеток</param>
        /// <param name="id">Индекс новой сетки (нужен для згрузки). По умолчаю находит свободный индекс.</param>
        /// <returns></returns>
        public GridState Add(uint gridSettingsId, uint? id = null)
        {
            var gridSettings = _level.GridSettingsCollection[gridSettingsId];
            GridStateCreateParams createParams = new( gridSettings, _dataLayerFabric );
            if (id.HasValue) {
                // TODO check not exists
                return _gridStateFabric.CreateWithCounter( createParams, id.Value );
            } else {
                return _gridStateFabric.Create( createParams );
            }
        }

        /// <summary>
        /// Удалить сетку со всем содержимым.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(uint id)
        {
            _gridStateRegistry.Remove( id );
        }

        public GridState this[uint id]
        {
            get {
                return _gridStateRegistry.Dict[id];
            }
        }

        public IEnumerator<GridState> GetEnumerator() => _gridStateRegistry.Values.GetEnumerator();

        private IEnumerator<GridState> GetEnumerator1() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();

        private void OnRegAdd(GridState gridState) => added?.Invoke( gridState );

        private void OnRegRemove(GridState gridState) => removed?.Invoke( gridState );

        private void OnFacCreate(GridState gridState) => _gridStateRegistry.Add( gridState );
    }
}