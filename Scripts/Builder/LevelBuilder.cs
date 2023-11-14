using Level.API;
using Level.IO;
using LevelView;
using UnityEngine;

namespace Level.Builder
{
    public interface IBuilderCheck
    {
        void Check();
    }

    public class LevelBuilder : MonoBehaviour, IBuilderCheck
    {
        [SerializeField] private LevelViewerEditor levelViewer;
        [SerializeField] public string levelFolder = "Levels";
        [SerializeField] private GridSettingsBuilder builderGridSettings;
        [SerializeField] private BlockProtosBuilder builderBlockProtos;
        [SerializeField] private GridInstanceBuilderCollector builderGridInstanceCollector;
        [SerializeField] private bool jsonPrettyPrint;

        public void ExportLevel()
        {
            ClearFolder();

            var level = new LevelAPIFabric().Create();

            BuildLevelState( level );

            var levelSaver = new FileLevelSaver( levelFolder, jsonPrettyPrint );
            level.TODORefactorSaveLevel( levelSaver );
        }

        public void ImportLevel()
        {
            // Clear all
            while (transform.childCount > 0) {
                DestroyImmediate( transform.GetChild( 0 ).gameObject );
            }

            builderGridSettings = new GameObject( "GridSettings", typeof( GridSettingsBuilder ) ).GetComponent<GridSettingsBuilder>();
            builderGridSettings.transform.parent = transform;

            builderBlockProtos = new GameObject( "BlockProtos", typeof( BlockProtosBuilder ) ).GetComponent<BlockProtosBuilder>();
            builderBlockProtos.transform.parent = transform;

            builderGridInstanceCollector = new GameObject( "Grids", typeof( GridInstanceBuilderCollector ) ).GetComponent<GridInstanceBuilderCollector>();
            builderGridInstanceCollector.transform.parent = transform;

            var level = new LevelAPIFabric().Create();
            var levelLoader = new FileLevelLoader( levelFolder );

            levelLoader.LoadFullContent( level );

            builderGridSettings.Import( level.GridSettingsCollection );
            builderBlockProtos.Import( level.BlockProtoCollection );
            builderGridInstanceCollector.Import( level.GridStatesCollection, level.BlockProtoCollection );
        }

        private void ClearFolder()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( levelFolder );
            foreach (var file in di.GetFiles()) {
                file.Delete();
            }
            foreach (var dir in di.GetDirectories()) {
                dir.Delete( true );
            }
        }

        private void BuildLevelState(LevelAPI level)
        {
            builderGridSettings.Export( level.GridSettingsCollection );
            builderBlockProtos.Export( level.BlockProtoCollection );
            builderGridInstanceCollector.Export( level.GridStatesCollection, level.BlockProtoCollection );
        }

        private void ImportLevelState(LevelAPI level)
        {
        }

        public void Check()
        {
            if (!builderGridSettings) {
                Debug.LogError( $"{this}: Missing {nameof( builderGridSettings )}" );
            }

            if (!builderBlockProtos) {
                Debug.LogError( $"{this}: Missing {nameof( builderBlockProtos )}" );
            }

            if (!builderGridInstanceCollector) {
                Debug.LogError( $"{this}: Missing {nameof( builderGridInstanceCollector )}" );
            }
        }

        public void RebuildView()
        {
            var level = new LevelAPIFabric().Create();
            BuildLevelState( level );
            levelViewer.SetModel( level );
        }

        public void RebuildViewRective()
        {
            var level = new LevelAPIFabric().Create();
            levelViewer.SetModel( level );
            BuildLevelState( level );
        }
    }
}