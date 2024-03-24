using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORPath))]
public class ORPathEditor : Editor {
	
    override public void OnInspectorGUI() {
    	
    	SharedInspectorGUI((ORPath)target);

    	if (GUI.changed)
            EditorUtility.SetDirty(target);

    }
    
    public static void SharedInspectorGUI(ORPath path) {
    	
    	path.alwaysDrawGizmos = EditorGUILayout.Toggle("Enable Gizmos:", path.alwaysDrawGizmos);
    	path.pathName = EditorGUILayout.TextField("Name:", path.pathName);
    	path.color = EditorGUILayout.ColorField("Color:", path.color);

    }
    
    public static ORPoint GetPoint(ORPoint point, Transform transform) {
    	return GetPoint(point, transform, "Point Type:");
    }
    
    public static ORPoint GetPoint(ORPoint point, Transform transform, string label) {
		
		bool allowSceneObjects = true;
    	
    	point.pointType = (ORPointType)EditorGUILayout.EnumPopup(label, point.pointType);

    	if (point.pointType == ORPointType.Vector3) {
    		
    		point.vector3Point = EditorGUILayout.Vector3Field("Position:", point.vector3Point);
    		
    	} else if (point.pointType == ORPointType.Transform) {
    		
    		point.transformPoint = (Transform)EditorGUILayout.ObjectField("Position:", point.transformPoint, typeof(Transform), allowSceneObjects);

    	} else if (point.pointType == ORPointType.Self) {
    		
    		point.transformPoint = transform;
    		
    	}

    	return point;

    }
    
}