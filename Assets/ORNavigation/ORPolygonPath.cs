using UnityEngine;
using System.Collections;

public class ORPolygonPath : ORSegmentedPath {
	
	// Override
	
	override public void DrawGizmos() {
		
		//base.DrawGizmos();

		Gizmos.color = color;

		if (points.Count == 0)
			return;

		ORPoint pointA = points[0];
		ORPoint pointB = null;

		for (int i = 1; i < points.Count; i++) {
			
			pointB = points[i];

			Gizmos.DrawLine(pointA.value, pointB.value);

			pointA = pointB;
			
		}
		
		//Gizmos.DrawLine(pointA.value, points[0].value);

	}

		
	override protected float CalculateSegmentLength(int segmentIndex) {
		
		Vector3 p0 = FirstSegmentPoint(segmentIndex);
		Vector3 p1 = SecondSegmentPoint(segmentIndex);
		
		return (p1 - p0).magnitude;
		
	}
	
	override protected Vector3 PointOnSegment(int segmentIndex, float t) {
		
		Vector3 p0 = FirstSegmentPoint(segmentIndex);
		Vector3 p1 = SecondSegmentPoint(segmentIndex);

		return p0 + (p1 - p0) * t;
		
	}

	
}
