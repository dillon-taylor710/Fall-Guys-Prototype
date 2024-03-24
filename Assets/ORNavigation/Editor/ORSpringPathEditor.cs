using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORSpringPath))]
public class ORSpringPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORSpringPath path = (ORSpringPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.height = EditorGUILayout.FloatField("Height:", path.height);
    	path.bottomRadius = EditorGUILayout.FloatField("Bottom Radius:", path.bottomRadius);
    	path.topRadius = EditorGUILayout.FloatField("Top Radius:", path.topRadius);
    	path.frequency = EditorGUILayout.FloatField("Frequency:", path.frequency);
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}