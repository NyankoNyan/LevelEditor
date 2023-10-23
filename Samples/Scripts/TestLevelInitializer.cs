using Level.IO;
using LevelView;
using UnityEngine;

namespace Level.Samples
{
    [DefaultExecutionOrder( 1 )]
    public class TestLevelInitializer : MonoBehaviour
    {
        [SerializeField] string _levelFolder = "Levels";

        private void Start()
        {
            ImportLevel();
        }

        private void ImportLevel()
        {
            var levelLoader = new FileLevelLoader( _levelFolder );
            LevelStorage.Instance.LoadAll( levelLoader );
        }
    }
}
