using UnityEngine;
using System;
using System.Collections;

public class ORiTweenPath : ORSegmentedPath {
	
	// Private

	private static Vector3[] PathControlPointGenerator(Vector3[] path) {
		
		Vector3[] suppliedPath;
		Vector3[] vector3s;
		
		//create and store path points:
		suppliedPath = path;

		//populate calculate path;
		int offset = 2;
		vector3s = new Vector3[suppliedPath.Length + offset];
		Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);
		
		//populate start and end control points:
		//vector3s[0] = vector3s[1] - vector3s[2];
		vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
		vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);
		
		//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
		if(vector3s[1] == vector3s[vector3s.Length-2]) {
			
			Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
			
			Array.Copy(vector3s, tmpLoopSpline, vector3s.Length);
			tmpLoopSpline[0] = tmpLoopSpline[tmpLoopSpline.Length - 3];
			tmpLoopSpline[tmpLoopSpline.Length - 1] = tmpLoopSpline[2];
			
			vector3s = new Vector3[tmpLoopSpline.Length];
			
			Array.Copy(tmpLoopSpline,vector3s,tmpLoopSpline.Length);
			
		}	
		
		return(vector3s);
		
	}
	
	//andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
	private static Vector3 Interp(Vector3[] pts, float t) {
		
		int numSections = pts.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
				
		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];
		
		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
		
	}

	// Override
	
	override public float PathLength() {
		
		int n = points.Count;
		
		if (n < 2)
			return 0;
		
		Vector3[] currentPath = new Vector3[n];
		
		for (int i = 0; i < n; i++)
			currentPath[i] = points[i].value;
		
		float pathLength = 0;
		
		Vector3[] vector3s = PathControlPointGenerator(currentPath);
		
		//Line Draw:
		Vector3 prevPt = Interp(vector3s, 0);
		int SmoothAmount = currentPath.Length * 20;
		
		for (int i = 1; i <= SmoothAmount; i++) {
			
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s, pm);
			
			pathLength += Vector3.Distance(prevPt, currPt);
			prevPt = currPt;
			
		}
		
		return pathLength;

	}
	
	override public Vector3 PointOnPath(float t) {
		
		int n = points.Count;
		
		if (n == 0)
			return Vector3.zero;
			
		if (n == 1)
			return points[0].value;
		
		Vector3[] currentPath = new Vector3[n];
		
		for (int i = 0; i < n; i++)
			currentPath[i] = points[i].value;
		
		return Interp(PathControlPointGenerator(currentPath), t);
		
	}

}
