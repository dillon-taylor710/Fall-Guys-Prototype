using UnityEngine;
using System.Collections;

public enum OROscillatorAxis {
	Custom,
	Forward,
	Right,
	Up
}

public class OROscillatorPath : ORPath {
	
	[SerializeField]
	private OROscillatorAxis _axis = OROscillatorAxis.Up;
	
	[SerializeField]
	private Vector3 _direction = Vector3.up;
	
	[SerializeField]
	private float _amplitude = 1;
	
	[SerializeField]
	private float _constant = 0;

	public OROscillatorAxis axis {
		get { return _axis; }
		set { _axis = value; }
	}

	public Vector3 direction {
		get { return _direction; }
		set { _direction = value; }
	}
	
	public float amplitude {
		get { return _amplitude; }
		set { _amplitude = value; }
	}

	public float constant {
		get { return _constant; }
		set { _constant = value; }
	}
		
	// Override
	
	override public Vector3 PointOnPath(float t) {

		float amount = _amplitude * Mathf.Cos(Mathf.PI * 2 * t) + constant;
		
		Vector3 direction = Vector3.zero;
		
		if (_axis == OROscillatorAxis.Forward) {
			
			direction = transform.forward;
			
		} else if (_axis == OROscillatorAxis.Right) {
			
			direction = transform.right;
			
		} else if (_axis == OROscillatorAxis.Up) {
			
			direction = transform.up;
			
		} else {
			
			direction = transform.TransformDirection(_direction.normalized);
			
		}

		return transform.position + direction * amount;
		
	}
	
}
