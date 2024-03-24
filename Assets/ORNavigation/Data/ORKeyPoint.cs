using UnityEngine;
using System.Collections;

[System.Serializable]
public class ORKeyPoint {
	
	[SerializeField]
	private ORKeyPoint _previous;
	
	[SerializeField]
	private ORKeyPoint _next;
	
	[SerializeField]
	private float _t;
	
	public ORKeyPoint previous {
		get { return _previous; }
		set { _previous = value; }
	}
	
	public ORKeyPoint next {
		get { return _next; }
		set { _next = value; }
	}
	
	public float t {
		get { return _t; }
		set {
			
			if (isValidTValue(value))
				_t = value;
			
		}
	}
	
	public float minT {
		get {
			
			if (_previous == null)
				return 0;
				
			return _previous.t;
			
		}
	}
	
	public float maxT {
		get {
			
			if (_next == null)
				return 1;
				
			return _next.t;
			
		}
	}
	
	// Lyfe cycle
	
	public ORKeyPoint() {
		Initialize(0);
	}
	
	public ORKeyPoint(float t) {
		Initialize(t);
	}
	
	// Private
	
	private void Initialize(float t) {
		_t = t;
		_previous = null;
		_next = null;
	}
	
	private bool isValidTValue(float t) {
		
		if (_t < 0 || _t > 1)
			return false;
		
		if (_previous != null && t <= _previous.t)
			return false;
			
		if (_next != null && t >= _next.t)
			return false;
		
		return true;
		
	}
	
}
