using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORCirclePath))]
public class ORCirclePathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORCirclePath path = (ORCirclePath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.radius = EditorGUILayout.FloatField("Radius:", path.radius);
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}