using UnityEngine;
using System.Collections;

public class ORCirclePath : OREllipsePath {
	
	// Variables
	
	[SerializeField]
	private float _radius = 1f;
	
	// Properties
	
	public float radius {
		get { return _radius; }
		set {
			
			_radius = value;
			
			semiMajorAxis = radius;
			semiMinorAxis = radius;
			
		}
	}
	
	// Override
	
	override public float PathLength() {
		return 2 * Mathf.PI * _radius;
	}

}
