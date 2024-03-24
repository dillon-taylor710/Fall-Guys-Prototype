using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OREllipsePath))]
public class OREllipsePathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	OREllipsePath path = (OREllipsePath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.semiMajorAxis = EditorGUILayout.FloatField("Semi Major Axis:", path.semiMajorAxis);
    	path.semiMinorAxis = EditorGUILayout.FloatField("Semi Minor Axis:", path.semiMinorAxis);
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}