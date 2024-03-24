using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORArcPath))]
public class ORArcPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORArcPath path = (ORArcPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.semiMajorAxis = EditorGUILayout.FloatField("Semi Major Axis:", path.semiMajorAxis);
    	path.semiMinorAxis = EditorGUILayout.FloatField("Semi Minor Axis:", path.semiMinorAxis);
    	path.start = EditorGUILayout.FloatField("Start:", path.start);
    	path.end = EditorGUILayout.FloatField("End:", path.end);
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}