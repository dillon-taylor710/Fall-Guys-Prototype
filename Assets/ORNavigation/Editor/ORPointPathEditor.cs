using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORPointPath))]
public class ORPointPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	ORPointPath path = (ORPointPath)target;
    	
    	ORPathEditor.SharedInspectorGUI(path);
    	
    	path.radius = EditorGUILayout.FloatField("Radius:", path.radius);    	
    	path.point = ORPathEditor.GetPoint(path.point, path.transform);

    	if (GUI.changed) {
    		
            EditorUtility.SetDirty(target);
            
    	}
            
    }
    
    public void OnSceneGUI () {
    	/*
    	Transform transform = ((ORPointPath)target).transform;
    	
        Handles.color = new Color(1, 0, 0, 0.1f);
        Handles.DrawSolidDisc(transform.position, transform.up, 5);
        
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.up, 4);
        
        transform.rotation =
        Handles.Disc(transform.rotation,
                    transform.position,
                    transform.up,
                    4,
                    false,
                    5);

		if (GUI.changed) {
    		
            EditorUtility.SetDirty(target);
            
    	}
    	*/
    }
   
}