using UnityEngine.Events;

namespace Level.API
{
    public interface IChunkAPI
    {
        public UnityAction onDestroy { get; set; }
    }

    internal class ChunkAPI
    {

    }
}
