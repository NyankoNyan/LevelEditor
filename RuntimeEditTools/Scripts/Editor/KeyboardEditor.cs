using RuntimeEditTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeEditTools
{
    [CustomEditor( typeof( Keyboard ) )]
    public class KeyboardEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Keyboard keyboard = (Keyboard)target;

            if (GUILayout.Button( "Update buttons" )) {
                keyboard.UpdateButtonsTexts();
            }
        }
    }

    //[CustomEditor( typeof( InputField ) )]
    //public class InputFieldEditor : Editor
    //{
    //    override 
    //}
}
