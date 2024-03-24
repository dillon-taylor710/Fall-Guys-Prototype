using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ORUpdateMode {
	Update,
	LateUpdate,
	FixedUpdate
}

public enum ORMovementMode {
	Velocity,
	Duration
}

public enum ORNavigationWrapMode {
	Once,
	Loop,
	PingPong
}

public enum ORLookMode {
	None,
	OrientToPath,
	Path,
	Point,
	Target
}

public enum ORNavigationDirection {
	Normal = 1,
	Reverse = -1
}

public class ORNavigation : MonoBehaviour {
	
	// Inspector variables

	public bool foldoutCuePoints = false;
	public int upSteps = 100;
	public float upDirectionsSize = 1f;
	public int upVectorSelectedIndex = 0;
	public float upVectorTPosition = 0f;
	
	// --
	
	[SerializeField]
	private bool _playOnAwake = true;
	
	[SerializeField]
	private string _navigationName = "";
	
	[SerializeField]
	private Transform _target;
	
	[SerializeField]
	private ORNavigationDirection _direction = ORNavigationDirection.Normal;
	
	[SerializeField]
	private ORUpdateMode _updateMode = ORUpdateMode.LateUpdate;
	
	[SerializeField]
	private ORNavigationWrapMode _wrapMode = ORNavigationWrapMode.Once;
	
	[SerializeField]
	private ORMovementMode _movementMode = ORMovementMode.Duration;
	
	[SerializeField]
	private ORLookMode _lookMode = ORLookMode.None;
	
	[SerializeField]
	private bool _customUpDirections = false;
	
	[SerializeField]
	private float _timeScale = 1f;
	
	[SerializeField]
	private float _velocity = 1f;
	
	[SerializeField]
	private float _duration = 1f;
	
	[SerializeField]
	private GameObject _navigationManager;
	
	[SerializeField]
	private AnimationCurve _tCurve = AnimationCurve.Linear(0, 0, 1, 1);
	
	[SerializeField]
	private List<ORCuePoint> _cuePoints = new List<ORCuePoint>();
	
	[SerializeField]
	private ORPath _movePath;
	
	[SerializeField]
	private ORPath _lookPath;

	[SerializeField]
	private Vector3 _lookPoint = Vector3.zero;
	
	[SerializeField]
	private Transform _lookTarget;
	
	[SerializeField]
	private List<ORUpVector> _upVectors = new List<ORUpVector>();

	private float _t;
	private int _tDirection;
	private float _durationBasedTVelocity;
	private float _velocityBasedTVelocity;
	private ArrayList _activatedCuePoints = new ArrayList();
	
	// Properties
	
	public Transform target {
		get { return _target; }
		set { _target = value; }
	}
	
	public ORNavigationDirection direction {
		get { return _direction; }
		set { _direction = value; }
	}
	
	public bool playOnAwake {
		get { return _playOnAwake; }
		set { _playOnAwake = value; }
	}
	
	public string navigationName {
		get { return _navigationName; }
		set { _navigationName = value; }
	}
	
	public ORUpdateMode updateMode {
		get { return _updateMode; }
		set { _updateMode = value; }
	}
	
	public ORNavigationWrapMode wrapMode {
		get { return _wrapMode; }
		set { _wrapMode = value; }
	}
	
	public ORMovementMode movementMode {
		get { return _movementMode; }
		set { _movementMode = value; }
	}
	
	public ORLookMode lookMode {
		get { return _lookMode; }
		set { _lookMode = value; }
	}
	
	public bool customUpDirections {
		get { return _customUpDirections; }
		set { _customUpDirections = value; }
	}
	
	public float timeScale {
		get { return _timeScale; }
		set { _timeScale = value; }
	}
	
	public float velocity {
		get { return _velocity; }
		set { _velocity = value; }
	}
	
	public float duration {
		get { return _duration; }
		set { _duration = value; }
	}
	
	public GameObject navigationManager {
		get { return _navigationManager; }
		set { _navigationManager = value; }
	}
	
	public List<ORCuePoint> cuePoints {
		get { return _cuePoints; }
		set { _cuePoints = value; }
	}
	
	public AnimationCurve tCurve {
		get { return _tCurve; }
		set { _tCurve = value; }
	}
	
	public ORPath movePath {
		get { return _movePath; }
		set { _movePath = value; }
	}
	
	public ORPath lookPath {
		get { return _lookPath; }
		set { _lookPath = value; }
	}
	
	public Vector3 lookPoint {
		get { return _lookPoint; }
		set { _lookPoint = value; }
	}
	
	public Transform lookTarget {
		get { return _lookTarget; }
		set { _lookTarget = value; }
	}
	
	public List<ORUpVector> upVectors {
		get { return _upVectors; }
	}
	
	public bool hasLookMode {
		get { return _lookMode != ORLookMode.None; }
	}
	
	public bool hasLookPoint {
		get { return _lookMode == ORLookMode.Point || _lookMode == ORLookMode.Path || _lookMode == ORLookMode.Target; }
	}
	
	public float t {
		get { return _t; }
		set {
			
			_t = value;
			
			for (int i = 0; i < _cuePoints.Count; i++)
				if (((ORCuePoint)_cuePoints[i]).t >= _t)
					_activatedCuePoints[i] = false;
			
		}
	}
	
	// MonoBehaviour
	
	private void Awake() {
		
		RegisterNavigation(this);
		
		Initialize();
		
		if (_playOnAwake) {

			Play();
			
		} else {
			
			enabled = false;
			
		}
		
	}
	
	private void OnDestroy() {
		UnregisterNavigation(this);
	}
	
	private void Update() {
		
		if (_updateMode == ORUpdateMode.Update)
			Navigate();
			
	}
	
	private void FixedUpdate() {
		
		if (_updateMode == ORUpdateMode.FixedUpdate)
			Navigate();
			
	}
	
	private void LateUpdate() {
		
		if (_updateMode == ORUpdateMode.LateUpdate)
			Navigate();
			
	}
	
	private void OnDrawGizmos() {
			
		Vector3 from = Vector3.zero;
		Vector3 to = Vector3.zero;
		
		if (_movePath != null) {
			
			_movePath.DrawGizmos();
		
			foreach (ORCuePoint cuePoint in _cuePoints) {
				
				from = _movePath.PointOnPath(cuePoint.t);
				to = GetLookPointAt(cuePoint.t);
				
				Gizmos.color = cuePoint.color;
				Gizmos.DrawWireSphere(from, .1f);
				
				if (hasLookPoint)
					Gizmos.DrawLine(from, to);
				
			}
			
			Gizmos.color = Color.black;
			
			if (hasLookPoint) {
				
				from = _movePath.PointOnPath(t);
				to = GetLookPointAt(_t);

				Gizmos.DrawLine(from, to);

			}
			
		}

		if (_lookPath != null && _lookMode == ORLookMode.Path) {
			
			_lookPath.DrawGizmos();
			
		} else if (_lookMode == ORLookMode.OrientToPath) {
			
			int steps = 20;
			float tInc = 1f / (float)20;
			
			for (int i = 0; i < steps; i++) {
				
				float t = i * tInc;
				
				from = _movePath.PointOnPath(t);
				to = GetLookPointAt(t);
				
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(from, to);
				
			}
			
		}
			
	}
	
	// Private
	
	private void Initialize() {
		_t = 0;
	}
	
	private bool CuePointHit(int index, float t) {
			
		if ((bool)_activatedCuePoints[index])
			return false;
		
		ORCuePoint cuePoint = (ORCuePoint)_cuePoints[index];
			
		if (_tDirection == 1)
			return t >= cuePoint.t;
			
		return t <= cuePoint.t;
		
	}
	
	private bool AnimationDidEnd(float t) {
		
		if (_tDirection == 1 && t >= 1)
			return true;
			
		if (_tDirection == -1 && t <= 0)
			return true;
		
		return false;
		
	}
	
	private void Navigate() {
		
		// Update T velocity
		
		CalculateTVelocity();
		
		// Update target position and rotation
		
		SetOnPath(_target, _t);
			
		// CuePoints

		for (int i = 0; i < _cuePoints.Count; i++) {

			if (CuePointHit(i, _t)) {
	
				if (_navigationManager != null) {

					_navigationManager.SendMessage(
						"OnCuePoint",
						new ORCuePointParams(this, _cuePoints[i], i, _t),
						SendMessageOptions.DontRequireReceiver
					);
					
				}

				_activatedCuePoints[i] = true;
				
			}
			
		}
		
		// Navigation increment
				
		_t += GetTIncrement(_tDirection, Time.deltaTime);
		
		// Navigation end
		
		if (AnimationDidEnd(t)) {
			
			if (_navigationManager != null)
				_navigationManager.SendMessage("OnNavigationEnd", this, SendMessageOptions.DontRequireReceiver);
			
			if (_wrapMode == ORNavigationWrapMode.Once) {
				
				Stop();
				
			} else if (_wrapMode == ORNavigationWrapMode.Loop) {
				
				if (_direction == ORNavigationDirection.Normal) {
					
					StartAnimationFromBeginning();
					
				} else {
					
					StartAnimationFromEnd();
					
				}
				
			} else if (_wrapMode == ORNavigationWrapMode.PingPong) {
				
				if (_tDirection == 1) {
					
					StartAnimationFromEnd();
					
				} else {
					
					StartAnimationFromBeginning();
					
				}

			}
			
		}
		
	}
		
	private Quaternion GetRotationAt(Vector3 position, float t, bool stopUpDirections) {
		
		Quaternion rotation = Quaternion.identity;
		
		switch (_lookMode) {
			
			case ORLookMode.OrientToPath: {
				
				Vector3 direction = _movePath.Derivative(t);
				
				if (_movePath != null && direction != Vector3.zero)
					rotation = Quaternion.LookRotation(direction);
					
				break;
				
			}
			
			case ORLookMode.Path: {
				
				Vector3 point = Vector3.zero;
				
				if (_lookPath != null)
					point = _lookPath.PointOnPath(t);

				rotation = Quaternion.LookRotation(point - position);
				
				break;

			}
			
			case ORLookMode.Point: {
				
				rotation = Quaternion.LookRotation(_lookPoint - position);
				
				break;

			}

			case ORLookMode.Target: {
				
				if (_lookTarget != null)
					rotation = Quaternion.LookRotation(_lookTarget.position - position);
					
				break;

			}
			
		}

		if (_customUpDirections && !stopUpDirections)
			rotation *= CustomRotationAt(position, t);
			
		return rotation;
		
	}

	private float GetTIncrement(float tDirection, float deltaTime) {
		
		if (_movementMode == ORMovementMode.Duration)
			return _durationBasedTVelocity * deltaTime * _timeScale * tDirection;
		
		return _velocityBasedTVelocity * deltaTime * _timeScale * tDirection;

	}

	
	private void StartAnimationFromBeginning() {
		
		_t = 0;
		_tDirection = 1;
		
		StartAnimation();
		
	}
	
	private void StartAnimationFromEnd() {
		
		_t = 1;
		_tDirection = -1;
		
		StartAnimation();
		
	}
	
	private void StartAnimation() {
		
		_activatedCuePoints.Clear();
		
		for (int i = 0; i < _cuePoints.Count; i++)
			_activatedCuePoints.Add(false);
		
		if (_navigationManager != null)
			_navigationManager.SendMessage("OnNavigationStart", this, SendMessageOptions.DontRequireReceiver);
		
		enabled = true;
		
	}
	
	private Quaternion RotationVectorAt(float t) {
		
		Quaternion rotation = Quaternion.identity;
			
		if (_lookMode == ORLookMode.OrientToPath) {
			
		}
		
		return rotation;
		
	}
	
	// Public
	
	public void AddCuePoint() {
		AddCuePoint(new ORCuePoint());
	}
	
	public void AddCuePoint(ORCuePoint cuePoint) {
		_cuePoints.Add(cuePoint);
		_activatedCuePoints.Add(false);
	}
		
	public void RemoveCuePoint(ORCuePoint cuePoint) {
		RemoveCuePointAtIndex(_cuePoints.IndexOf(cuePoint));
	}
	
	public void RemoveCuePointAtIndex(int index) {
		_cuePoints.RemoveAt(index);
		_activatedCuePoints.RemoveAt(index);
	}
	
	public void RemoveCuePoint(string name) {
		
		int i = 0;
		
		foreach (ORCuePoint aCuePoint in _cuePoints) {
			
			if (aCuePoint.name == name) {
				
				RemoveCuePointAtIndex(i);
				
				return;
				
			}
			
			i++;
			
		}

	}
	
	public void RemoveCuePointsByName(string name, int count) {
		
		ORCuePoint aCuePoint = null;
		int removed = 0;
		int i = 0;
		
		while (removed < count && i < _cuePoints.Count) {
			
			aCuePoint = (ORCuePoint)_cuePoints[i];
			
			if (aCuePoint.name == name) {
				
				RemoveCuePointAtIndex(i);
				
				removed++;
				
			} else {
				
				i++;
				
			}

		}
		
	}
	
	public void RemoveCuePoints() {
		_cuePoints.Clear();
	}
	
	// UpVectors
	
	public int AddUpVector(float t, ORUpVector up) {
		return AddKeyPoint(_upVectors, t, up);
	}
	
	public void RemoveUpVectorAtIndex(int index) {
		RemoveKeyPointAtIndex(_upVectors, index);
	}
	
	public void RemoveUpVectors() {
		_upVectors.Clear();
	}
	
	// Controls
	
	public void Play() {
		
		CalculateTVelocity();
		
		if (t > 0 && t < 1) {
			
			enabled = true;
			
		} else {
			
			if (_direction == ORNavigationDirection.Normal) {
				
				StartAnimationFromBeginning();
				
			} else {
				
				StartAnimationFromEnd();
				
			}
			
		}
		
	}
	
	public void Pause() {
		enabled = false;
	}
	
	public void Stop() {
		
		_t = 0;
		
		enabled = false;
		
	}
	/*
	public void Seek(float t) {
	}
	*/
	
	public Vector3 GetPositionAt(float t) {
		
		if (_movePath == null)
			return Vector3.zero;
		
		if (_movementMode == ORMovementMode.Duration)
			return _movePath.PointOnPath(_tCurve.Evaluate(t));
			
		return _movePath.PointOnPath(t);
		
	}
	
	public Vector3 GetLookPointAt(float t) {
		
		if (_lookPath == null || !hasLookMode)
			return Vector3.zero;
		
		switch (_lookMode) {
			
			case ORLookMode.OrientToPath: {
				
				if (_movePath == null)
					return Vector3.zero;
					
				Vector3 derivative = _movePath.Derivative(t);
				
				if (derivative == Vector3.zero)
					return Vector3.zero;
					
				Vector3 position = _movePath.PointOnPath(t);
				
				return position + Quaternion.LookRotation(derivative) * Vector3.forward;
				
			}
			
			case ORLookMode.Path: {
				
				return _lookPath.PointOnPath(t);

			}
			
			case ORLookMode.Point: {
				
				return _lookPoint;

			}

			case ORLookMode.Target: {
				
				if (_lookTarget == null)
					return Vector3.zero;
				
				return _lookTarget.position;

			}
			
		}
		
		return Vector3.zero;
		
	}
	
	public Quaternion GetRotationAt(float t) {
		
		Vector3 position = Vector3.zero;

		if (_movePath != null) {
			
			position = _movePath.PointOnPath(t);
			
		} else if (_target != null) {
			
			position = _target.position;
			
		}
		
		return GetRotationAt(position, t, false);
		
	}
		
	public float GetTIncrement(ORNavigationDirection direction) {
		return GetTIncrement(direction, Time.deltaTime);
	}
		
	public float GetTIncrement(ORNavigationDirection direction, float deltaTime) {
		
		if (direction == ORNavigationDirection.Normal)
			return GetTIncrement(1, deltaTime);
			
		return GetTIncrement(-1, deltaTime);
		
	}
	
	public void SetOnPath(Transform transform, float t) {
		
		// Move
		
		transform.position = GetPositionAt(t);
		
		// Look

		if (hasLookMode)
			transform.rotation = GetRotationAt(transform.position, t, false);

	}
	
	public void SetOnPath(GameObject gameObject, float t) {
		SetOnPath(gameObject.transform, t);
	}
	
	// Key Frames
	
	private int GetKeyPosition(IList list, float t) {
		
		int n = list.Count;
		
		for (int i = 0; i < n; i++) {
			
			ORKeyPoint keyPoint = (ORKeyPoint)list[i];
			
			if (t == keyPoint.t)
				return -1;
				
			if (t < keyPoint.t)
				return i;

		}
		
		return n;
		
	}
	
	private int AddKeyPoint(IList list, float t, ORKeyPoint keyPoint) {
		
		int index = GetKeyPosition(list, t);
		
		if (index < 0)
			return -1;
		
		ORKeyPoint previous = null;
		ORKeyPoint next = null;
		
		if (index > 0 && index - 1 < list.Count)
			previous = (ORKeyPoint)list[index - 1];
			
		if (index < list.Count)
			next = (ORKeyPoint)list[index];
			
		if (previous != null)
			previous.next = keyPoint;
			
		if (next != null)
			next.previous = keyPoint;
		
		keyPoint.previous = previous;
		keyPoint.next = next;
		
		if (index == list.Count) {
			
			list.Add(keyPoint);
			
		} else {
			
			list.Insert(index, keyPoint);
			
		}
		
		return index;
		
	}
	
	private void RemoveKeyPointAtIndex(IList list, int index) {
		
		ORKeyPoint previous = null;
		ORKeyPoint next = null;
		
		if (index > 0)
			previous = (ORKeyPoint)list[index - 1];
			
		if (index + 1 < list.Count)
			next = (ORKeyPoint)list[index + 1];
			
		if (previous != null)
			previous.next = next;
			
		if (next != null)
			next.previous = previous;
		
		list.RemoveAt(index);
		
	}
	
	private Quaternion CustomRotationAt(Vector3 position, float t) {
		
		for (int i = 0; i < _upVectors.Count - 1; i++) {
			
			if (t >= _upVectors[i].t && t <= _upVectors[i + 1].t) {
				
				float localT = (t - _upVectors[i].t) / (_upVectors[i + 1].t - _upVectors[i].t);
				
				float angle = Mathf.Lerp(
					_upVectors[i].angle,
					_upVectors[i + 1].angle,
					localT
				);
				
				return Quaternion.AngleAxis(angle, Vector3.forward);

			}

		}
		
		return Quaternion.identity;

	}
	
	// Public
	
	public void CalculateTVelocity() {
		
		if (_movementMode == ORMovementMode.Duration) {
			
			_durationBasedTVelocity = 1 / _duration;
			
		} else {
			
			float movePathLength = _movePath.PathLength();

			if (movePathLength > 0) {
				
				_velocityBasedTVelocity = _velocity / movePathLength;
				
			} else {
				
				_velocityBasedTVelocity = 1;
				
			}
			
		}

	}
	
	// Static
	
	private static List<ORNavigation> _navigations = new List<ORNavigation>();
	
	private static void RegisterNavigation(ORNavigation navigation) {
		_navigations.Add(navigation);
	}
	
	private static void UnregisterNavigation(ORNavigation navigation) {
		_navigations.Remove(navigation);
	}
	
	public static ORNavigation GetNavigationByName(string name) {
		
		foreach (ORNavigation aNavigation in _navigations)
			if (aNavigation.navigationName == name)
				return aNavigation;
		
		return null;
		
	}
	
}
