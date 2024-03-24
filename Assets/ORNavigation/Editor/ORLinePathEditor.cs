using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORLinePath))]
public class ORLinePathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORLinePath path = (ORLinePath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);

    	path.p0 = ORPathEditor.GetPoint(path.p0, path.transform, "Point A Type:");
    	path.p1 = ORPathEditor.GetPoint(path.p1, path.transform, "Point B Type:");

    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}