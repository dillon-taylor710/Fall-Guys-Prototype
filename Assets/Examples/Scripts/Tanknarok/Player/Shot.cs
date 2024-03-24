using Fusion;
using FusionHelpers;
using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public struct ShotState : ISparseState<Shot>
	{
		/// <summary>
		/// Generic sparse state properties required by the interface
		/// </summary>
		public int StartTick { get; set; }
		public int EndTick { get; set; }

		/// <summary>
		/// Shot specific sparse properties
		/// </summary>
		public Vector3 Position;
		public Vector3 Direction;

		public ShotState(Vector3 startPosition, Vector3 direction)
		{
			StartTick = 0;
			EndTick = 0;
			Position = startPosition;
			Direction = direction;
		}

		public void Extrapolate(float t, Shot prefab)
		{
			Position = GetPositionAt(t, prefab);
			Direction = GetDirectionAt(t, prefab);
		}

		public Vector3 GetTargetPosition(Shot prefab)
		{
			float a = 0.5f * prefab.Gravity.y;
			float b = prefab.Speed * Direction.y;
			float c = Position.y;
			float d = b * b - 4 * a * c;
			float t = (-b - Mathf.Sqrt(d))/(2*a);
			Vector3 p = GetPositionAt(t, prefab);
			p.y = 0.05f; // Return the position with a slight y offset to avoid placing target where it will end up z-fighting with the ground;
			return p;
		}

		private Vector3 GetPositionAt(float t, Shot prefab) => Position + t * (prefab.Speed * Direction + 0.5f * t * prefab.Gravity);
		private Vector3 GetDirectionAt(float t, Shot prefab) => prefab.Speed==0 ? Direction : (prefab.Speed * Direction + t * prefab.Gravity).normalized;
	}
	
	public class Shot : MonoBehaviour, ISparseVisual<ShotState, Shot>
	{
		[Header("Settings")] 
		[SerializeField] private  Vector3 _gravity;
		[SerializeField] private  float _speed;
		[SerializeField] private  float _radius; 
		[SerializeField] private  LayerMask _hitMask;
		[SerializeField] private  float _range;
		[SerializeField] private  float _areaRadius;
		[SerializeField] private  float _areaImpulse;
		[SerializeField] private  byte _areaDamage;
		[SerializeField] private  float _timeToLive;
		[SerializeField] private bool _isHitScan;
		
		[Header("Fx Prefabs")] 
		[SerializeField] private ExplosionFX _detonationPrefab;
		[SerializeField] private TargetMarker _targetPrefab;
		[SerializeField] private MuzzleFlash _muzzleFxPrefab;

		public Vector3 Gravity => _gravity;
		public float Speed => _speed;
		public float Radius => _radius; 
		public LayerMask HitMask => _hitMask;
		public float Range => _range;
		public float AreaRadius => _areaRadius;
		public float AreaImpulse => _areaImpulse;
		public byte AreaDamage => _areaDamage;
		public float TimeToLive => _timeToLive;
		public bool IsHitScan => _isHitScan;

		private Transform _xform;

		private void Awake()
		{
			_xform = transform;
		}

		public void ApplyStateToVisual(NetworkBehaviour owner, ShotState state, float t, bool isFirstRender, bool isLastRender)
		{
			if(isLastRender)
			{
				// Slightly hacky, but we never move the hitscan so its current position is always the muzzle, and target is start+direction
				if (IsHitScan) 
					LocalObjectPool.Acquire(_detonationPrefab, state.Position+state.Direction, Quaternion.identity);
				else
					LocalObjectPool.Acquire(_detonationPrefab, state.Position, Quaternion.identity);
			}
			if (isFirstRender)
			{
				if (_targetPrefab)
					LocalObjectPool.Acquire(_targetPrefab, state.GetTargetPosition(this), Quaternion.identity);
				if (_muzzleFxPrefab)
					LocalObjectPool.Acquire(_muzzleFxPrefab, state.Position, Quaternion.LookRotation(state.Direction), owner.transform).OnFire(state);
			}
			_xform.forward = state.Direction;
			_xform.position = state.Position;
		}
	}
}