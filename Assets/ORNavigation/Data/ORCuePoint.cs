using UnityEngine;
using System.Collections;

[System.Serializable]
public class ORCuePoint {
	
	[SerializeField]
	private Color _color = Color.green;
	
	[SerializeField]
	private float _t;
	
	[SerializeField]
	private string _name;
	
	[SerializeField]
	private Object _data;
	
	// Properties
	
	public Color color {
		get { return _color; }
		set { _color = value; }
	}
	
	public float t {
		get { return _t; }
		set { _t = value; }
	}
	
	public string name {
		get { return _name; }
		set { _name = value; }
	}
	
	public Object data {
		get { return _data; }
		set { _data = value; }
	}
	
	// Lyfe cycle
	
	public ORCuePoint() {
		Initialize(0, "", null);
	}
	
	public ORCuePoint(float t) {
		Initialize(t, "", null);
	}
	
	public ORCuePoint(string name) {
		Initialize(0, name, null);
	}
	
	public ORCuePoint(Object data) {
		Initialize(0, "", data);
	}
	
	public ORCuePoint(float t, string name) {
		Initialize(0, name, null);
	}
	
	public ORCuePoint(float t, Object data) {
		Initialize(0, "", data);
	}
	
	// Private
	
	private void Initialize(float t, string name, Object data) {
		_t = t;
		_name = name;
		_data = data;
	}
	
}
