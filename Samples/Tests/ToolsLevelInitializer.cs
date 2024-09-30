using LevelView;

using RuntimeEditTools;

using UnityEngine;

namespace Level.Samples
{
    [DefaultExecutionOrder(1)]
    public class ToolsLevelInitializer : MonoBehaviour
    {
        private void Start()
        {
            ImportLevel();

            var blocksWorkbench = FindObjectOfType<BlocksWorkbenchFacade>();
            blocksWorkbench.Init();
        }

        private void ImportLevel()
        {
            LevelStorage.Instance.LoadAll();
        }
    }
}