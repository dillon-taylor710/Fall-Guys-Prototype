using UnityEngine;
using System.Collections;

public enum ORWaveFunction {
	Cos,
	Sin
}

public class ORWavePath : ORPath {
	
	[SerializeField]
	private ORWaveFunction _waveFunction = ORWaveFunction.Cos;
	
	[SerializeField]
	private float _minAngle = -3 * Mathf.PI;
	
	[SerializeField]
	private float _maxAngle = 3 * Mathf.PI;
	
	[SerializeField]
	private float _amplitude = 1;
	
	[SerializeField]
	private float _angularFrequency = 1;
	
	[SerializeField]
	private float _phase = 0;
	
	[SerializeField]
	private float _constant = 0;

	public ORWaveFunction waveFunction {
		get { return _waveFunction; }
		set { _waveFunction = value; }
	}
	
	public float minAngle {
		get { return _minAngle; }
		set { _minAngle = value; }
	}

	public float maxAngle {
		get { return _maxAngle; }
		set { _maxAngle = value; }
	}
	
	public float amplitude {
		get { return _amplitude; }
		set { _amplitude = value; }
	}
	
	public float angularFrequency {
		get { return _angularFrequency; }
		set { _angularFrequency = value; }
	}

	public float phase {
		get { return _phase; }
		set { _phase = value; }
	}

	public float constant {
		get { return _constant; }
		set { _constant = value; }
	}
		
	// Override
	
	override public Vector3 PointOnPath(float t) {
		
		float radians = _minAngle + (_maxAngle - _minAngle) * t;
		float angle = _angularFrequency * radians + _phase;
		
		float y = _amplitude;
		
		if (_waveFunction == ORWaveFunction.Cos) {
			
			y *= Mathf.Cos(angle);
			
		} else if (_waveFunction == ORWaveFunction.Sin) {
			
			y *= Mathf.Sin(angle);
			
		}
		
		return transform.position + transform.up * (y + constant) + transform.right * radians;// new Vector3(radians, y, 0);
		
	}
	
}
