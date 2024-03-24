using Fusion;
using FusionExamples.Utility;
using FusionHelpers;
using UnityEngine;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// The Weapon class controls how fast a weapon fires, which projectiles it uses
	/// and the start position and direction of projectiles.
	/// </summary>

	public class Weapon : NetworkBehaviourWithState<Weapon.NetworkState>
	{
		[Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
		public struct NetworkState : INetworkStruct
		{
			[Networked, Capacity(12)] 
			public NetworkArray<ShotState> bulletStates => default;
		}

		[SerializeField] private Transform[] _gunExits;
		[SerializeField] private float _rateOfFire;
		[SerializeField] private byte _ammo;
		[SerializeField] private bool _infiniteAmmo;
		[SerializeField] private LaserSightLine _laserSight;
		[SerializeField] private PowerupType _powerupType = PowerupType.DEFAULT;
		[SerializeField] private Shot _bulletPrefab;

		private SparseCollection<ShotState, Shot> bullets;
		private float _visible;
		private bool _active;
		private Collider[] _areaHits = new Collider[4];
		private Player _player;

		public float delay => _rateOfFire;
		public bool isShowing => _visible >= 1.0f;
		public byte ammo => _ammo;
		public bool infiniteAmmo => _infiniteAmmo;

		public PowerupType powerupType => _powerupType;

		private void Awake()
		{
			_player = GetComponentInParent<Player>();
		}

		public override void Spawned()
		{
			bullets = new SparseCollection<ShotState, Shot>(State.bulletStates, _bulletPrefab);
		}

		public override void FixedUpdateNetwork()
		{
			bullets.Process( this, (ref ShotState bullet, int tick) =>
			{
				if (bullet.Position.y < -.15f)
				{
					bullet.EndTick = Runner.Tick;
					return true;
				}

				if (!_bulletPrefab.IsHitScan && bullet.EndTick>Runner.Tick)
				{
					Vector3 dir = bullet.Direction.normalized;
					float length = Mathf.Max(_bulletPrefab.Radius, _bulletPrefab.Speed * Runner.DeltaTime);
					if(Physics.Raycast(bullet.Position - length * dir, dir, out var hitinfo, length, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore))
//					if (Runner.LagCompensation.Raycast(bullet.Position - length*dir, dir, length, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX))
					{ 
						bullet.Position = hitinfo.point;
						bullet.EndTick = Runner.Tick;
						ApplyAreaDamage(hitinfo.point);
						return true;
					}
				}
				return false;
			});
		}

		public override void Render()
		{
			if (TryGetStateChanges(out var from, out var to))
				OnFireTickChanged();
			else
				TryGetStateSnapshots(out from, out _, out _, out _, out _);

			bullets.Render(this, from.bulletStates );
		}

		/// <summary>
		/// Control the visual appearance of the weapon. This is controlled by the Player based
		/// on the currently selected weapon, so the boolean parameter is entirely derived from a
		/// networked property (which is why nothing in this class is sync'ed).
		/// </summary>
		/// <param name="show">True if this weapon is currently active and should be visible</param>
		public void Show(bool show)
		{
			if (_active && !show)
			{
				ToggleActive(false);
			}
			else if (!_active && show)
			{
				ToggleActive(true);
			}

			_visible = Mathf.Clamp(_visible + (show ? Time.deltaTime : -Time.deltaTime) * 5f, 0, 1);

			if (show)
				transform.localScale = Tween.easeOutElastic(0, 1, _visible) * Vector3.one;
			else
				transform.localScale = Tween.easeInExpo(0, 1, _visible) * Vector3.one;
		}

		private void ToggleActive(bool value)
		{
			_active = value;

			if (_laserSight != null)
			{
				if (_active)
				{
					_laserSight.SetDuration(0.5f);
					_laserSight.Activate();
				}
				else
					_laserSight.Deactivate();
			}
		}

		public void Fire(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
		{
			if (powerupType == PowerupType.EMPTY || _gunExits.Length == 0)
				return;
			
			Transform exit = GetExitPoint(Runner.Tick);
			
			Debug.DrawLine(exit.position, exit.position+exit.forward, Color.blue, 1.0f);
			Debug.Log($"Bullet fired in tick {runner.Tick} from position {exit.position} weapon is at {transform.position}");
			SpawnNetworkShot(runner, owner, exit, ownerVelocity);
		}

		private void OnFireTickChanged()
		{
			// Recharge the laser sight if this weapon has it
			if (_laserSight != null)
				_laserSight.Recharge();
		}

		private void SpawnNetworkShot(NetworkRunner runner, PlayerRef owner, Transform exit, Vector3 ownerVelocity)
		{
			if (_bulletPrefab.IsHitScan)
			{
				bool impact;
				Vector3 hitPoint = exit.position + _bulletPrefab.Range * exit.forward;
				if (runner.GameMode == GameMode.Shared)
				{
					impact = runner.GetPhysicsScene().Raycast(exit.position, exit.forward,out var hitinfo, _bulletPrefab.Range, _bulletPrefab.HitMask.value);
					hitPoint = hitinfo.point;
				}
				else
				{
					impact = Runner.LagCompensation.Raycast(exit.position, exit.forward, _bulletPrefab.Range, Object.InputAuthority, out var hitinfo, _bulletPrefab.HitMask.value, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);
					hitPoint = hitinfo.Point;
				}
				
				if (impact)
				{
					ApplyAreaDamage(hitPoint);
				}
				
				bullets.Add( runner, new ShotState(exit.position, hitPoint-exit.position), 0);
			}
			else
				bullets.Add(runner, new ShotState(exit.position, exit.forward), _bulletPrefab.TimeToLive);
		}

		private void ApplyAreaDamage(Vector3 hitPoint)
		{
			int cnt = Physics.OverlapSphereNonAlloc(hitPoint, _bulletPrefab.AreaRadius, _areaHits, _bulletPrefab.HitMask.value, QueryTriggerInteraction.Ignore);
			if (cnt > 0)
			{
				for (int i = 0; i < cnt; i++)
				{
					GameObject other = _areaHits[i].gameObject;
					if (other)
					{
						Player target = other.GetComponent<Player>();
						if (target != null && target!=_player )
						{
							Vector3 impulse = other.transform.position - hitPoint;
							float l = Mathf.Clamp(_bulletPrefab.AreaRadius - impulse.magnitude, 0, _bulletPrefab.AreaRadius);
							impulse = _bulletPrefab.AreaImpulse * l * impulse.normalized;
							target.RaiseEvent(new Player.DamageEvent { impulse=impulse, damage=_bulletPrefab.AreaDamage});
						}
					}
				}
			}
		}

		public Transform GetExitPoint(int tick)
		{
			Transform exit = _gunExits[tick% _gunExits.Length];
			return exit;
		}
	}
}