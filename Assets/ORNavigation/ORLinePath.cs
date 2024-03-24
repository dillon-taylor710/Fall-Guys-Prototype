using UnityEngine;
using System.Collections;

public class ORLinePath : ORSegmentedPath {
	
	[SerializeField]
	private ORPoint _p0;
	
	[SerializeField]
	private ORPoint _p1;
	
	public ORPoint p0 {
		get { return _p0; }
		set { _p0 = value; }
	}

	public ORPoint p1 {
		get { return _p1; }
		set { _p1 = value; }
	}
	
	// Override
	
	override public float PathLength() {
		return (_p1.value - _p0.value).magnitude;
	}
	
	override public Vector3 PointOnPath(float t) {
		return _p0.value + (_p1.value - _p0.value) * t;
	}
	
}
