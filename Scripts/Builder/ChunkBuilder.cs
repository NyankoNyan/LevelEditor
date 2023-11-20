using Level.API;
using UnityEngine;

namespace Level.Builder
{
    //public class ChunkBuilder : MonoBehaviour
    //{
    //    [SerializeField] private Vector3Int chunkCoord;

    //    public void Export(GridState gridState, BlockProtoCollection blockProtos)
    //    {
    //        foreach (var layerBuilder in GetComponentsInChildren<LayerBuilder>()) {
    //            layerBuilder.Export( gridState, chunkCoord, blockProtos );
    //        }
    //    }

    //    public void Import(GridState gridState, BlockProtoCollection blockProtos)
    //    {
    //        LevelAPITools.Clear( transform );
    //        foreach (var layerSettings in gridState.GridSettings.Settings.layers) {
    //            var go = new GameObject( layerSettings.tag );
    //            go.transform.parent = transform;
    //            var lb = go.AddComponent<LayerBuilder>();
    //            lb.Import( gridState, chunkCoord, blockProtos, layerSettings.layerType, layerSettings.tag );
    //        }
    //    }
    //}
}