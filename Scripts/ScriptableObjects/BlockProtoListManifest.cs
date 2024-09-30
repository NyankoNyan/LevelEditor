using Level;

using UnityEngine;

namespace LevelView
{
    [CreateAssetMenu(fileName = "BlockProtoList", menuName = "LevelEditor/BlockProtoList")]
    public class BlockProtoListManifest : ScriptableObject
    {
        [SerializeField] public BlockProtoSettings[] Blocks;
    }
}