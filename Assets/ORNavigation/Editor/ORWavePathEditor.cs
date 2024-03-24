using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORWavePath))]
public class ORWavePathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORWavePath path = (ORWavePath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.waveFunction = (ORWaveFunction)EditorGUILayout.EnumPopup("Function:", path.waveFunction);
    	path.minAngle = EditorGUILayout.FloatField("Min Angle:", path.minAngle);
    	path.maxAngle = EditorGUILayout.FloatField("Max Angle:", path.maxAngle);
    	path.amplitude = EditorGUILayout.FloatField("Amplitude:", path.amplitude);
    	path.angularFrequency = EditorGUILayout.FloatField("Angular Frequency:", path.angularFrequency);
    	path.phase = EditorGUILayout.FloatField("Phase:", path.phase);
    	path.constant = EditorGUILayout.FloatField("Constant:", path.constant);

    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}