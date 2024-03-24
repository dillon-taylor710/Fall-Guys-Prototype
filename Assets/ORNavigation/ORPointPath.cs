using UnityEngine;
using System.Collections;

public class ORPointPath : ORPath {
	
	[SerializeField]
	public ORPoint point = new ORPoint();
	
	[SerializeField]
	private float _radius = 1;
	
	public float radius {
		get { return _radius; }
		set { _radius = value; }
	}
		
	// Override
	
	override public Vector3 PointOnPath(float t) {
		return point.value;
	}
	
	override public void DrawGizmos() {
		Gizmos.color = color;
		Gizmos.DrawWireSphere(point.value, _radius);
	}
	
}
