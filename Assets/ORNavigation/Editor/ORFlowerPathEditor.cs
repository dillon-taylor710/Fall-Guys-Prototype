using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORFlowerPath))]
public class ORFlowerPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORFlowerPath path = (ORFlowerPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.radius = EditorGUILayout.FloatField("Radius:", path.radius);
    	path.distance = EditorGUILayout.FloatField("Distance:", path.distance);
    	path.frequency = EditorGUILayout.FloatField("Frequency:", path.frequency);
    	path.origin = ORPathEditor.GetPoint(path.origin, path.transform, "Origin:");
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}