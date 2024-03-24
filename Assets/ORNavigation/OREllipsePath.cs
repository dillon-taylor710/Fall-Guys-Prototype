using UnityEngine;
using System.Collections;

public class OREllipsePath : ORPath {
	
	[SerializeField]
	private float _a = 1f;
	
	[SerializeField]
	private float _b = 2f;
	
	// Properties
		
	public float semiMajorAxis {
		get { return _a; }
		set { _a = value; }
	}
	
	public float semiMinorAxis {
		get { return _b; }
		set { _b = value; }
	}
	
	// Override
		
	override public float PathLength() {
		return Mathf.PI * (_a + _b);
	}

	override public Vector3 PointOnPath(float t) {
		
		// Source: http://www.physicsforums.com/showthread.php?t=123168
		
		float radians = Mathf.PI * 2 * t; 
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		return _a * cos * transform.right + _b * sin * transform.forward + transform.position;
		
	}
		
}
