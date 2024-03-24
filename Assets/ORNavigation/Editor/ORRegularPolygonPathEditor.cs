using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORRegularPolygonPath))]
public class ORRegularPolygonPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORRegularPolygonPath path = (ORRegularPolygonPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.sides = EditorGUILayout.IntField("Sides:", path.sides);
    	path.radius = EditorGUILayout.FloatField("Radius:", path.radius);
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}