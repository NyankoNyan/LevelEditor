using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Level.Builder.Editor
{
    [CustomEditor( typeof( LevelBuilder ) )]
    public class LevelBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            LevelBuilder lb = (LevelBuilder)target;

            if (GUILayout.Button( "Change level folder" )) {
                string value = EditorUtility.OpenFolderPanel( "Choose level folder", lb.levelFolder, "" );

                if (!string.IsNullOrWhiteSpace( value )) {
                    string projectFolder = Application.dataPath.Substring( 0, Application.dataPath.LastIndexOf( "Assets" ) );

                    if (value.Length >= projectFolder.Length
                        && value.Substring( 0, projectFolder.Length ) == projectFolder) {
                        value = value.Substring( projectFolder.Length );
                    }

                    lb.levelFolder = value;
                }
            }

            if (GUILayout.Button( "Export level" )) {
                lb.ExportLevel();
            }

            if (GUILayout.Button( "Import level" )) {
                lb.ImportLevel();
            }

            if (GUILayout.Button( "Check all" )) {
                var allChecks = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany( x => x.GetComponentsInChildren<IBuilderCheck>() );
                foreach (var builderCheck in allChecks) {
                    builderCheck.Check();
                }
            }

            if (GUILayout.Button( "Update view (as preset)" )) {
                lb.RebuildView();
            }

            if (GUILayout.Button( "Update view (reactive)" )) {
                lb.RebuildViewRective();
            }
        }
    }
}
