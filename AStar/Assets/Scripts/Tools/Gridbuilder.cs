using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pathfinding;

[ExecuteInEditMode]
[CustomEditor(typeof(VisibleGrid))]
public class Gridbuilder : Editor
{
    public override void OnInspectorGUI()
    {
       DrawDefaultInspector();

       VisibleGrid script = (VisibleGrid)target;
       if(GUILayout.Button("Build Grid"))
       {
           script.CreateGrid();
       } 

       if(GUILayout.Button("Destroy Grid"))
       {
           script.DestroyGridInEditor();
       }
    }
}
