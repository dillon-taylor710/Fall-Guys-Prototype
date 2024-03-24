using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ORStarPath : ORPolygonPath {
	
	[SerializeField]
	private int _sides = 3;
	
	[SerializeField]
	private float _radius = 1f;
	
	[SerializeField]
	private float _slope = .5f;
	
	[SerializeField]
	private float _starPathLength = 0f;

	// Properties
	
	public int sides {
		get { return _sides; }
		set {
			
			_sides = value;
			
			if (_sides < 3) {
				
				_sides = 3;
				
			} else if (_sides > 10) {
				
				_sides = 10;
				
			}
				
			generatePointsForPolygon(_sides, _radius);
			
		}
	}
	
	public float radius {
		get { return _radius; }
		set {
			
			_radius = value;
			
			generatePointsForPolygon(_sides, _radius);
			
		}
	}
	
	public float slope {
		get { return _slope; }
		set { _slope = value; }
	}
	
	// Private
	
	private void generatePointsForPolygon(int sides) {
		generatePointsForPolygon(sides, 1f);
	}
	
	private void generatePointsForPolygon(int sides, float radius) {

		if (sides < 2)
			return;
		
		RemovePoints();

		float angle = 360f / (float)sides;
		float middleAngle = 0;
		Vector3 middlePoint = Vector3.zero;
		
		for (int i = 0; i < sides; i++) {
			
			AddPoint(
				new ORPoint(
					Quaternion.AngleAxis(angle * i, transform.up) * (transform.right * _radius) + transform.position
				)
			);
			
			middleAngle = (angle * i) + (angle / 2f);
			middlePoint = Quaternion.AngleAxis(middleAngle, transform.up) * (transform.right * _radius * _slope);
			middlePoint += transform.position;
			
			AddPoint(new ORPoint(middlePoint));

		}
		
		AddPoint(new ORPoint(points[0].vector3Point));
		
		_starPathLength = (points[1].vector3Point - points[0].vector3Point).magnitude * sides * 2;

	}
	
	// Override
	
	override public float PathLength() {
		return _starPathLength;
	}

}
