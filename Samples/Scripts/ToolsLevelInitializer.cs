using Level.IO;
using LevelView;
using RuntimeEditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Samples
{
    [DefaultExecutionOrder( 1 )]
    public class ToolsLevelInitializer : MonoBehaviour
    {
        [SerializeField] string _levelFolder = "Levels";

        private void Start()
        {
            ImportLevel();

            var blocksWorkbench = FindObjectOfType<BlocksWorkbenchFacade>();
            blocksWorkbench.Init();
        }

        private void ImportLevel()
        {
            var levelLoader = new FileLevelLoader( _levelFolder );
            LevelStorage.Instance.LoadAll( levelLoader );
        }
    }
}
