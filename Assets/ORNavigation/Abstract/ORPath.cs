using UnityEngine;
using System.Collections;

public class ORPath : MonoBehaviour {
	
	public bool alwaysDrawGizmos = true;
	
	[SerializeField]
	private string _pathName = "";
	
	[SerializeField]
	private Color _color = Color.yellow;
	
	// Properties
	
	public string pathName {
		get { return _pathName; }
		set { _pathName = value; }
	}
	
	public Color color {
		get { return _color; }
		set { _color = value; }
	}
	
	// Lyfe cycle
	
	public void Awake() {
		
		Initialize();
		
		enabled = false;
		
	}
	
	// MonoBehaviour
	
	private void OnDrawGizmos() {
		
		if (alwaysDrawGizmos)
			DrawGizmos();
			
	}
	
	private void OnDrawGizmosSelected() {
		DrawGizmos();
	}
	
	// Protected
	
	virtual protected void Initialize() {
	}
	
	// Public
	
	virtual public ORPath Clone() {
		return null;
	}
	
	virtual public float PathLength() {

		float step = .01f;
		int stepsCount = (int)(1f / step);
		
		Vector3 pointA = PointOnPath(0);
		Vector3 pointB = Vector3.zero;
		
		float length = 0;

		for (int i = 1; i < stepsCount; i++) {

			pointB = PointOnPath(i * step);
			
			length += (pointB - pointA).magnitude;

			pointA = pointB;
			
		}
		
		return length;
		
	}
	
	virtual public Vector3 Derivative(float t) {

		Vector3 pointA = PointOnPath(t);
		Vector3 pointB = PointOnPath(t + .01f);
		
		Vector3 derivative = pointB - pointA;
		
		return derivative;
		
	}
	
	virtual public Vector3 PointOnPath(float t) {
		return Vector3.zero;
	}
	
	virtual public void DrawGizmos() {
		
		Gizmos.color = color;
		
		float step = .01f;
		int stepsCount = (int)(1f / step);
		
		Vector3 pointA = PointOnPath(0);
		Vector3 pointB = Vector3.zero;

		for (int i = 1; i < stepsCount; i++) {

			pointB = PointOnPath(i * step);

			Gizmos.DrawLine(pointA, pointB);

			pointA = pointB;
			
		}
		
		Gizmos.DrawLine(pointA, PointOnPath(1));
		
	}
	
	virtual public Vector3 PutOnPath(Transform target, float t) {
		
		Vector3 point = PointOnPath(t);
		
		target.position = PointOnPath(t);
		
		return point;
		
	}
		
}
