using UnityEngine;
using UnityEngine.Serialization;

namespace FusionExamples.Tanknarok
{
	public class MotorShake : MonoBehaviour
	{
		[FormerlySerializedAs("shakeAmountByAxis")] [SerializeField] private Vector3 _shakeAmountByAxis = Vector3.zero;
		[FormerlySerializedAs("shakeSpeed")] [SerializeField] private float _shakeSpeed = 10f;

		private float _offset;
		private Vector3 _originScale;

		void Start()
		{
			_originScale = transform.localScale;
			_offset = Random.Range(-Mathf.PI, Mathf.PI);
		}

		Vector3 CalculateShake()
		{
			Vector3 shake = new Vector3(Mathf.Sin(Time.time * _shakeSpeed + _offset), Mathf.Sin(Time.time * _shakeSpeed + _offset), Mathf.Sin(Time.time * _shakeSpeed + _offset));
			shake.x *= _shakeAmountByAxis.x;
			shake.y *= _shakeAmountByAxis.y;
			shake.z *= _shakeAmountByAxis.z;
			return shake;
		}

		void Update()
		{
			transform.localScale = _originScale + CalculateShake();
		}
	}
}