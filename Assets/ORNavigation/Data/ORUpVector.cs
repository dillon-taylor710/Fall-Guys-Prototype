using UnityEngine;
using System.Collections;

[System.Serializable]
public class ORUpVector : ORKeyPoint {
	
	[SerializeField]
	private float _angle = 0;

	public float angle {
		get { return _angle; }
		set { _angle = value; }
	}
	
	// Lyfe cycle
	
	public ORUpVector(float t) : base(t) {
	}
	
}
