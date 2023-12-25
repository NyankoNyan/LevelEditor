using Level.API;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Builder
{
    public class BlockProtosBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] private List<BlockProtoSettings> _blockProtoParams = new();

        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<LevelBuilder>()) {
                Debug.LogError( $"{this}: Required {nameof( LevelBuilder )} as parent" );
            }
        }

        public void Export(BlockProtoCollection blockProtos)
        {
            foreach (var bp in _blockProtoParams) {
                blockProtos.Add( bp );
            }
        }

        public void Import(BlockProtoCollection blockProtos)
        {
            _blockProtoParams.Clear();
            foreach (var bp in blockProtos) {
                _blockProtoParams.Add( bp.Settings );
            }
        }
    }
}