using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORiTweenPath))]
public class ORiTweenPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORiTweenPath path = (ORiTweenPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	for (int i = 0; i < path.pointsCount; i++) {
    		
	    	EditorGUILayout.Space();
		    EditorGUILayout.PrefixLabel("#" + i + " Point");
    		path.points[i] = ORPathEditor.GetPoint(path.points[i], path.transform);
    		
	    	EditorGUILayout.BeginHorizontal();
	    		
		    if (GUILayout.Button("Add Point Below"))
		    	path.AddPointAtIndex(new ORPoint(ORPointType.Transform), i + 1);
	    
		    if (GUILayout.Button("Remove"))
	    		path.RemovePointAt(i);
		    	
		    EditorGUILayout.EndHorizontal();

    	}
    	
	    EditorGUILayout.Space();
	    
	    if (path.pointsCount == 0) {
	    	
		    if (GUILayout.Button("Add Point"))
		    	path.AddPoint(new ORPoint(ORPointType.Transform));
		    	
	    } else {
	    	
		    if (GUILayout.Button("Remove All Points"))
		    	path.RemovePoints();

	    }
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);
            
    }
   
}