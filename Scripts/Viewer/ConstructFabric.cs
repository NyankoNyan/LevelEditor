using Level.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelView
{
    public class ConstructFabric
    {
        private Dictionary<string, ObjectSetup> _prefabs = new();
        private Dictionary<string, List<ObjectSetup>> _refIdIndex = new();

        /// <summary>
        /// Добавляет в коллекцию префаб с уникальным идентификатором.
        /// </summary>
        /// <param name="prefabId">Идентификатор</param>
        /// <param name="go">Объект префаба</param>
        /// <exception cref="LevelAPIException"></exception>
        public void AddPrefab(ObjectSetup objectSetup)
        {
            if (_prefabs.TryGetValue( objectSetup.id, out ObjectSetup currentOS )) {
                if (objectSetup.Equals( currentOS )) {
                    Debug.LogWarning( $"Prefab {objectSetup.id} already added" );
                } else {
                    throw new LevelAPIException( $"Prefab {objectSetup.id} conflicts with another prefab {currentOS.id}" );
                }
            } else {
                _prefabs.Add( objectSetup.id, objectSetup );
                List<ObjectSetup> indexList = null;
                if (!_refIdIndex.TryGetValue( objectSetup.refId, out indexList )) {
                    indexList = new();
                    _refIdIndex.Add( objectSetup.refId, indexList );
                }
                indexList.Add( objectSetup );
            }
        }

        public void AddPrefabs(IEnumerable<ObjectSetup> objectSetups)
        {
            foreach (var objectSetup in objectSetups) {
                AddPrefab( objectSetup );
            }
        }

        public GameObject Create(string refId)
        {
            if (_refIdIndex.TryGetValue( refId, out List<ObjectSetup> indexList )) {
                //Ну мы пока просто первое совпадение берём. А потом доп.папраметры будут.
                var objectSetup = indexList[0];
                if (objectSetup.prefab) {
                    return GameObject.Instantiate( objectSetup.prefab );
                } else {
                    throw new NotImplementedException();
                }
            } else {
                throw new LevelAPIException( $"Missing prefab with id {refId}" );
            }
        }

        public bool HasRefId(string refId)
        {
            return _refIdIndex.ContainsKey( refId );
        }

        public bool HasPrefab(string id)
            => _prefabs.ContainsKey( id );
    }
}