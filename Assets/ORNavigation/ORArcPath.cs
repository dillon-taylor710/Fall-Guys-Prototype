using UnityEngine;
using System.Collections;

public class ORArcPath : OREllipsePath {
	
	[SerializeField]
	private float _start = 0f;
	
	[SerializeField]
	private float _end = 180f;
	
	[SerializeField]
	private float _angleRatio;
	
	[SerializeField]
	private float _tOffset;
	
	// Properties
	
	public float start {
		get { return _start; }
		set {
			
			_start = value;
			
			_tOffset = _start / 360f;
			
			RecalculateAngleRatio();
			
		}
	}
	
	public float end {
		get { return _end; }
		set {
			_end = value;
			RecalculateAngleRatio();
		}
	}
	
	// Private
	
	override protected void Initialize() {
		RecalculateAngleRatio();
	}
	
	private void RecalculateAngleRatio() {
		_angleRatio = (_end - _start) / (Mathf.PI * 2 * Mathf.Rad2Deg);
	}
	
	// Override
	
	override public Vector3 PointOnPath(float t) {
		return base.PointOnPath(t * _angleRatio + _tOffset);
	}

}
