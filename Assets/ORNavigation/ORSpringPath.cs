using UnityEngine;
using System.Collections;

public class ORSpringPath : ORPath {
	
	[SerializeField]
	private float _height;
	
	[SerializeField]
	private float _bottomRadius = 1;
	
	[SerializeField]
	private float _topRadius = 1;
	
	[SerializeField]
	private float _frequency = 1;
	
	public float height {
		get { return _height; }
		set { _height = value; }
	}
	
	public float bottomRadius {
		get { return _bottomRadius; }
		set { _bottomRadius = value; }
	}
	
	public float topRadius {
		get { return _topRadius; }
		set { _topRadius = value; }
	}
	
	public float frequency {
		get { return _frequency; }
		set { _frequency = value; }
	}

	// Override
	
	override public Vector3 PointOnPath(float t) {
		
		float radius = _bottomRadius + (_topRadius - _bottomRadius) * t;
		
		Vector3 point = new Vector3(
			radius * Mathf.Cos(t * _frequency),
			_height * t,
			radius * Mathf.Sin(t * _frequency)
		);

		return transform.position + transform.rotation * point;
		
	}
	
}
