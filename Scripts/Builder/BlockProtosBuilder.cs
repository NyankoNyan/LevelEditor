using System.Collections.Generic;
using UnityEngine;
using Level.API;

namespace Level.Builder
{
    public class BlockProtosBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] List<BlockProtoSettings> _blockProtoParams = new();

        public void Check()
        {
            if (!transform.parent && !transform.parent.GetComponent<LevelBuilder>()) {
                Debug.LogError( $"{this}: Required {nameof( LevelBuilder )} as parent" );
            }
        }

        public void Export(IBlockProtoAPI api)
        {
            foreach (var bp in _blockProtoParams) {
                api.Add( bp );
            }
        }

        public void Import(IBlockProtoAPI api)
        {
            _blockProtoParams.Clear();
            foreach (var bp in api.BlockProtos) {
                _blockProtoParams.Add( bp.Settings );
            }
        }
    }
}
