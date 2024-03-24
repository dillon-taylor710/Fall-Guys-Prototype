using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ORSegmentedPath : ORPath {
	
	[SerializeField]
	private List<ORPoint> _points = new List<ORPoint>();
	
	[SerializeField]
	private List<float> _distances = new List<float>();
	
	[SerializeField]
	private List<float> _offsets = new List<float>();
	
	[SerializeField]
	private List<float> _accumulatedOffsets = new List<float>();
	
	[SerializeField]
	private int _pointsCount;
	
	[SerializeField]
	private int _segmentsCount;
	
	[SerializeField]
	private float _pathLength;
	
	// Properties
	
	public List<ORPoint> points {
		get { return _points; }
	}
	
	public List<float> distances {
		get { return _distances; }
	}
	
	public List<float> offsets {
		get { return _offsets; }
	}
	
	public int pointsCount {
		get { return _points.Count; }
	}
	
	public int segmentsCount {
		get { return _segmentsCount; }
	}
	
	// Private
	
	private void UpdateSegmentsLength() {
		
		if (_points.Count == 0)
			return;
		
		_pathLength = 0;

		for (int i = 0; i < _points.Count; i++) {

			_distances[i] = CalculateSegmentLength(i);
			
			if (i < _points.Count - 1)
				_pathLength += _distances[i];

		}

	}
	
	private void UpdateOffsets() {
		
		if (_points.Count == 0)
			return;
			
		_accumulatedOffsets[0] = 0;

		for (int i = 0; i < _points.Count; i++) {
			
			_offsets[i] = _distances[i] / _pathLength;
			
			if (i > 0)
				_accumulatedOffsets[i] = _accumulatedOffsets[i - 1] + _offsets[i - 1];

		}

	}
	
	// Protected
	
	virtual protected float CalculateSegmentLength(int segmentIndex) {
		return 0f;
	}
	
	virtual protected Vector3 PointOnSegment(int segmentIndex, float t) {
		return Vector3.zero;
	}
	
	virtual protected Vector3 FirstSegmentPoint(int segmentIndex) {
		return _points[segmentIndex].value;
	}
	
	virtual protected Vector3 SecondSegmentPoint(int segmentIndex) {

		if (segmentIndex < _points.Count - 1)
			return _points[segmentIndex + 1].value;

		return _points[0].value;
		
	}
	
	// Override
	
	override public float PathLength() {
		return _pathLength;
	}
	
	override public Vector3 PointOnPath(float t) {
		
		UpdateSegmentsLength();
		UpdateOffsets();
		
		int n = _points.Count - 1;
		
		for (int i = 0; i < n; i++)
			if (_accumulatedOffsets[i + 1] > t)
				return PointOnSegment(i, (t - _accumulatedOffsets[i]) / _offsets[i]);

		return points[0].value;
		
	}
	
	// Public
	
	public void AddPoint(ORPoint point) {
		
		_points.Add(point);
		_distances.Add(0);
		_offsets.Add(0);
		_accumulatedOffsets.Add(0);

	}
	
	public void AddPointAtIndex(ORPoint point, int index) {
		
		_points.Insert(index, point);
		_distances.Insert(index, 0);
		_offsets.Insert(index, 0);
		_accumulatedOffsets.Insert(index, 0);
		
	}
	
	public void RemovePointAt(int index) {
		
		_points.RemoveAt(index);
		_distances.RemoveAt(index);
		_offsets.RemoveAt(index);
		_accumulatedOffsets.RemoveAt(index);

	}
	
	public void RemoveLastPoint() {
		RemovePointAt(_points.Count - 1);
	}
	
	public void RemovePoints() {
		_points.Clear();
		_distances.Clear();
		_offsets.Clear();
		_accumulatedOffsets.Clear();
	}

}
