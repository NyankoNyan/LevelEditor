using LevelView;

using UnityEngine;

namespace Level.Samples
{
    [DefaultExecutionOrder(1)]
    public class TestLevelInitializer : MonoBehaviour
    {
        private void Start()
        {
            ImportLevel();
        }

        private void ImportLevel()
        {
            LevelStorage.Instance.LoadAll();
        }
    }
}