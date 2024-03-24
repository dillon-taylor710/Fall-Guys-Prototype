using UnityEngine;
using System.Collections;

public enum ORPointType {
	Vector3,
	Transform,
	Self
}

[System.Serializable]
public class ORPoint {
	
	[SerializeField]
	private ORPointType _pointType;
	
	[SerializeField]
	private Vector3 _vector3Point;
	
	[SerializeField]
	private Transform _transformPoint;

	public ORPointType pointType {
		get { return _pointType; }
		set { _pointType = value; }
	}
	
	public Vector3 vector3Point {
		get { return _vector3Point; }
		set { _vector3Point = value; }
	}
	
	public Transform transformPoint {
		get { return _transformPoint; }
		set { _transformPoint = value; }
	}
	
	public Vector3 value {
		get {
			
			if (_pointType == ORPointType.Vector3)
				return _vector3Point;
			
			if (_transformPoint == null)
				return Vector3.zero;
				
			return _transformPoint.position;
			
		}
	}
	
	// Lyfe cycle
	
	public ORPoint() {
		_pointType = ORPointType.Vector3;
		_vector3Point = Vector3.zero;
	}
	
	public ORPoint(Vector3 point) {
		_pointType = ORPointType.Vector3;
		_vector3Point = point;
	}
	
	public ORPoint(Transform transform) {
		_pointType = ORPointType.Transform;
		_transformPoint = transform;
	}
	
	public ORPoint(ORPointType type) {
		_pointType = type;
		_vector3Point = Vector3.zero;
		_transformPoint = null;
	}
	
}
