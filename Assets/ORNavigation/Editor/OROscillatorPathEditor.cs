using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OROscillatorPath))]
public class OROscillatorPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	OROscillatorPath path = (OROscillatorPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.axis = (OROscillatorAxis)EditorGUILayout.EnumPopup("Axis:", path.axis);
    	
    	if (path.axis == OROscillatorAxis.Custom)
    		path.direction = EditorGUILayout.Vector3Field("Direction:", path.direction);
    	
    	path.amplitude = EditorGUILayout.FloatField("Amplitude:", path.amplitude);
    	path.constant = EditorGUILayout.FloatField("Constant:", path.constant);

    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}