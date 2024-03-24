using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORStarPath))]
public class ORStarPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORStarPath path = (ORStarPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.sides = EditorGUILayout.IntField("Sides:", path.sides);
    	path.radius = EditorGUILayout.FloatField("Radius:", path.radius);
    	path.slope = EditorGUILayout.FloatField("Slope:", path.slope);
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}