using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public class LaserBeam : MonoBehaviour
	{
		[Header("Laser Beam Settings")] [SerializeField]
		private byte _damage;

		[SerializeField] private LayerMask _collisionMask;
		[SerializeField] private float _range;

		[Header("Effects")] 
		[SerializeField] private LineRenderer _laser;
		[SerializeField] private ParticleSystem _spark;
		[SerializeField] private ParticleSystem _muzzleFlash;

		private Player _target;
		private Vector3 _targetPoint;

		public void Init()
		{
			_muzzleFlash.Play();
			_spark.Play();
		}

		public void Render(bool applyDamage)
		{
			Vector3 pos = transform.position;
			_laser.SetPosition(0, pos);

			if (FindTarget(out RaycastHit hit))
			{
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
				{
					_target = hit.collider.GetComponentInParent<Player>();
					_targetPoint = hit.point;
				}
			}
		
			if (applyDamage && _target != null)
			{
				Vector3 impulse = _target.transform.position - _targetPoint;
				float l = Mathf.Clamp(5.0f - impulse.magnitude, 0, 5.0f);
				impulse = 10.0f * l * impulse.normalized;
				_target.ApplyAreaDamage(impulse, _damage);
				_target = null;
			}

			_spark.transform.position = hit.point;
			_laser.SetPosition(1, hit.point);
		}

		private bool FindTarget(out RaycastHit hit)
		{
			Vector3 forward = transform.rotation * Vector3.forward;
			if (Physics.Raycast(transform.position, forward, out hit, _range, _collisionMask))
				return true;
			hit.point = transform.position + forward * _range;
			return false;
		}
	}
}