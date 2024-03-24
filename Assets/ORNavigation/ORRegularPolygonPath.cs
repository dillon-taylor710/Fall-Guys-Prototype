using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ORRegularPolygonPath : ORPolygonPath {
	
	[SerializeField]
	private int _sides = 3;
	
	[SerializeField]
	private float _radius = 1f;
	
	[SerializeField]
	private float _tSide;
	
	[SerializeField]
	private float _regularPathLength = 0f;
	
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
	
	// Private
	
	public void generatePointsForPolygon(int sides) {
		generatePointsForPolygon(sides, 1f);
	}
	
	public void generatePointsForPolygon(int sides, float radius) {

		if (sides < 2)
			return;
		
		RemovePoints();

		float angle = 360f / (float)sides;

		_tSide = 1f / (float)sides;
		
		for (int i = 0; i < sides; i++) {
			
			AddPoint(
				new ORPoint(
					Quaternion.AngleAxis(angle * i, transform.up) * (transform.right * _radius) + transform.position
				)
			);

		}
		
		AddPoint(new ORPoint(points[0].vector3Point));
		
		_regularPathLength = (points[1].vector3Point - points[0].vector3Point).magnitude * sides;

	}
	
	// Override
	
	override public float PathLength() {
		return _regularPathLength;
	}

	override public Vector3 PointOnPath(float t) {

		if (points.Count == 0)
			generatePointsForPolygon(_sides, _radius);
		
		int side = (int)(t / _tSide);
		
		return PointOnSegment(side, (t - (_tSide * side)) / _tSide);
	
	}

}
