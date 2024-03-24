using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ORNavigation))]
public class ORNavigationEditor : Editor {
	
    override public void OnInspectorGUI () {
		
		bool allowSceneObjects = true;

    	ORNavigation navigation = (ORNavigation)target;
    	
    	navigation.playOnAwake = EditorGUILayout.Toggle("Play On Awake:", navigation.playOnAwake);
    	navigation.navigationName = EditorGUILayout.TextField("Navigation Name:", navigation.navigationName);
    	navigation.target = (Transform)EditorGUILayout.ObjectField("Target:", navigation.target, typeof(Transform), allowSceneObjects);
    	navigation.direction = (ORNavigationDirection)EditorGUILayout.EnumPopup("Direction:", navigation.direction);
    	navigation.updateMode = (ORUpdateMode)EditorGUILayout.EnumPopup("Update Function:", navigation.updateMode);
    	navigation.wrapMode = (ORNavigationWrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", navigation.wrapMode);
    	navigation.movementMode = (ORMovementMode)EditorGUILayout.EnumPopup("Movement Mode:", navigation.movementMode);
    	
    	if (navigation.movementMode == ORMovementMode.Duration) {
    		
    		navigation.duration = EditorGUILayout.FloatField("Duration:", navigation.duration);
	    	navigation.tCurve = EditorGUILayout.CurveField("Time Curve:", navigation.tCurve);
    		
    	} else if (navigation.movementMode == ORMovementMode.Velocity) {
    		
    		navigation.velocity = EditorGUILayout.FloatField("Velocity:", navigation.velocity);
    		
    	}
    	
    	navigation.timeScale = EditorGUILayout.FloatField("Time Scale:", navigation.timeScale);
    	
    	navigation.navigationManager = (GameObject)EditorGUILayout.ObjectField("Manager:", navigation.navigationManager, typeof(GameObject), allowSceneObjects);
	    	
	    navigation.movePath = (ORPath)EditorGUILayout.ObjectField("Move Path:", navigation.movePath, typeof(ORPath), allowSceneObjects);
    	navigation.lookMode = (ORLookMode)EditorGUILayout.EnumPopup("Look Mode:", navigation.lookMode);

    	if (navigation.lookMode == ORLookMode.Path) {
    		
	    	navigation.lookPath = (ORPath)EditorGUILayout.ObjectField("Look Path:", navigation.lookPath, typeof(ORPath), allowSceneObjects);
	    
	    } else if (navigation.lookMode == ORLookMode.Point) {
	    	
	    	navigation.lookPoint = EditorGUILayout.Vector3Field("Point:", navigation.lookPoint);
	    	
    	} else if (navigation.lookMode == ORLookMode.Target) {
    		
    		navigation.lookTarget = (Transform)EditorGUILayout.ObjectField("Target:", navigation.lookTarget, typeof(Transform), allowSceneObjects);
    	
    	}
    	
    	navigation.customUpDirections = EditorGUILayout.Toggle("Up Directions:", navigation.customUpDirections);
    	
    	if (navigation.customUpDirections) {
    		
    		// Orient to path
    		
		    navigation.upSteps = EditorGUILayout.IntSlider("Up Samples:", navigation.upSteps, 2, 200);
		    navigation.upDirectionsSize = EditorGUILayout.Slider("Up Size:", navigation.upDirectionsSize, .1f, 100f);
		    
	    	EditorGUILayout.BeginHorizontal();
	    	
		    navigation.upVectorTPosition = EditorGUILayout.Slider("Add At Position:", navigation.upVectorTPosition, 0f, 1f);
	    	
	    	if (GUILayout.Button("Add Key Frame")) {
	    		
	    		int index = navigation.AddUpVector(navigation.upVectorTPosition, new ORUpVector(navigation.upVectorTPosition));
	    		
	    		if (index < 0) {
	    			
	    			EditorUtility.DisplayDialog("Cannot add key frame", "There is another key frame at the specified position", "OK");
	    			
	    		} else {
	    			
	    			navigation.upVectorSelectedIndex = index;
	    			
	    		}
	    		
	    	}
		    	
	    	EditorGUILayout.EndHorizontal();
	    	
	    	if (navigation.upVectors.Count > 0) {
	    	
		    	if (navigation.upVectorSelectedIndex >= navigation.upVectors.Count)
		    		navigation.upVectorSelectedIndex = navigation.upVectors.Count - 1;
	    		
		    	string[] upVectorsList = new string[navigation.upVectors.Count];
		    	
		    	for (int i = 0; i < navigation.upVectors.Count; i++)
		    		upVectorsList[i] = "#" + i + " Up Vector (t = " + navigation.upVectors[i].t + ")";
		    	
		    	navigation.upVectorSelectedIndex = EditorGUILayout.Popup("Index:", navigation.upVectorSelectedIndex, upVectorsList);
		    	
		    	ORUpVector upVector = navigation.upVectors[navigation.upVectorSelectedIndex];
		    	
			    upVector.t = EditorGUILayout.Slider("Position:", upVector.t, upVector.minT, upVector.maxT); 
			    upVector.angle = EditorGUILayout.Slider("Angle:", upVector.angle, -360, 360);

		    	EditorGUILayout.BeginHorizontal();
		    	
		    	if (GUILayout.Button("Remove Key Frame"))
		    		navigation.RemoveUpVectorAtIndex(navigation.upVectorSelectedIndex);
		    	
		    	if (GUILayout.Button("Remove All Key Frames"))
			    	navigation.RemoveUpVectors();
			    	
		    	EditorGUILayout.EndHorizontal();
		    	
	    	}

    	}
    	
    	EditorGUILayout.Space();
    	
    	navigation.foldoutCuePoints = EditorGUILayout.Foldout(navigation.foldoutCuePoints, "CuePoints:");
    	
    	if (navigation.foldoutCuePoints) {
    		
    		int i = 0;
    		
    		while (i < navigation.cuePoints.Count) {
    			
    			ORCuePoint cuePoint = (ORCuePoint)navigation.cuePoints[i];
    		
		    	EditorGUILayout.Space();
		        EditorGUILayout.PrefixLabel("#" + i + " Cue Point");
		        cuePoint.color = EditorGUILayout.ColorField("Color:", cuePoint.color);
		    	cuePoint.t = EditorGUILayout.Slider("Timeline:", cuePoint.t, 0f, 1f);
		    	cuePoint.name = EditorGUILayout.TextField("Label:", cuePoint.name);
		    	cuePoint.data = EditorGUILayout.ObjectField("Object:", cuePoint.data, typeof(Object), allowSceneObjects);
		    	
		    	if (GUILayout.Button("Remove")) {
		    		
		    		navigation.RemoveCuePointAtIndex(i);
		    		
		    	} else {
		    	
			    	i++;
			    	
		    	}
		    	
    		}
    		
	    	EditorGUILayout.Space();
	    	EditorGUILayout.BeginHorizontal();
	    	
	    	if (GUILayout.Button("Add Cue Point"))
	    		navigation.AddCuePoint();
	    	
	    	if (navigation.cuePoints.Count > 0 && GUILayout.Button("Remove All Cue Points"))
		    	navigation.RemoveCuePoints();
		    	
	    	EditorGUILayout.EndHorizontal();
    		
    	}
  
    	// Update editor
    	
    	if (GUI.changed)
            EditorUtility.SetDirty(target);

    }
    
    public void OnSceneGUI () {

    	ORNavigation navigation = (ORNavigation)target;
    	
    	if (navigation.customUpDirections) {
    		
    		ORPath movePath = navigation.movePath;
    		Vector3 position = Vector3.zero;
    		Vector3 derivative = Vector3.zero;
    		Quaternion currentRotation = Quaternion.identity;
    		float t = 0;
    		
    		for (int i = 0; i < navigation.upVectors.Count; i++) {
    			
    			ORUpVector up = navigation.upVectors[i];

    			position = movePath.PointOnPath(up.t);
    			derivative = movePath.Derivative(up.t);

		        Handles.color = new Color(1, 0, 0, 0.2f);
		        Handles.DrawSolidDisc(position, derivative, .2f);

    			currentRotation = navigation.GetRotationAt(up.t);

		        Handles.color = new Color(1, 1, 0, 1f);
		        Handles.DrawLine(position, position + currentRotation * Vector3.up * 2);

    		}
    		
    		float tInc = 1f / (float)navigation.upSteps;
    		
    		for (int i = 0; i < navigation.upSteps; i++) {
    			
    			t = tInc * i;
    			
    			position = movePath.PointOnPath(t);
    			currentRotation = navigation.GetRotationAt(t);
    			
		        Handles.color = Color.green;
		        Handles.DrawLine(position, position + currentRotation * Vector3.up * navigation.upDirectionsSize);

    		}
    		
    	}
    	
    }

}