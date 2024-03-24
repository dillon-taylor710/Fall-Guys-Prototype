using UnityEngine;
using System.Collections;

public class ORFlowerPath : ORPath {

	[SerializeField]
	private float _radius = 1;
	
	[SerializeField]
	private float _distance = .3f;
	
	[SerializeField]
	private float _frequency = .2f;
	
	[SerializeField]
	private ORPoint _origin = new ORPoint(ORPointType.Self);
	
	public float radius {
		get { return _radius; }
		set { _radius = value; }
	}
	
	public float distance {
		get { return _distance; }
		set { _distance = value; }
	}
	
	public float frequency {
		get { return _frequency; }
		set { _frequency = value; }
	}
	
	public ORPoint origin {
		get { return _origin; }
		set { _origin = value; }
	}
		
	// Override
	
	override public Vector3 PointOnPath(float t) {
				
		float angle = t * Mathf.PI * 2;
		
		Vector3 point = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, transform.up) * (transform.right * _radius);
		point += transform.position;
				
		Vector3 direction = (_origin.value - point).normalized;
		point += direction * _distance * Mathf.Cos(angle / _frequency);

		return point;

	}
	
}
