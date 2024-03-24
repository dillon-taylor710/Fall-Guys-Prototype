using Fusion;
using FusionHelpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FusionExamples.Tanknarok
{
	public class PowerupSpawner : NetworkBehaviourWithState<PowerupSpawner.NetworkState> 
	{
		[Networked] public override ref NetworkState State => ref MakeRef<NetworkState>();
		public struct NetworkState : INetworkStruct
		{
			public TickTimer respawnTimer;
			public int activePowerupIndex;
		}

		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;
		[SerializeField] private MeshRenderer _rechargeCircle;

		[Header("Colors")] 
		[SerializeField] private Color _mainPowerupColor;
		[SerializeField] private Color _specialPowerupColor;
		[SerializeField] private Color _buffPowerupColor;

		const float RESPAWN_TIME = 3f;

		private static readonly int Recharge = Shader.PropertyToID("_Recharge");
		private AudioEmitter _audio;

		public override void Spawned()
		{
			_audio = GetComponent<AudioEmitter>();
			if(Object.HasStateAuthority)
				SetNextPowerup();
			InitPowerupVisuals();
		}

		public override void Render()
		{
			if ( TryGetStateChanges(out var old, out var current) )
			{
				if(old.respawnTimer.TargetTick>0) // Avoid triggering sound effect on initial init
					_audio.PlayOneShot(GetPowerup(old.activePowerupIndex).pickupSnd);
				InitPowerupVisuals();
			}

			float progress = 0;
			if (!State.respawnTimer.Expired(Runner))
				progress = 1.0f - (State.respawnTimer.RemainingTime(Runner) ?? 0) / RESPAWN_TIME;
			else
				_renderer.transform.localScale = Vector3.Lerp(_renderer.transform.localScale, Vector3.one, Time.deltaTime * 5f);
			_rechargeCircle.material.SetFloat(Recharge, progress);
		}

		private void OnTriggerStay(Collider collisionInfo)
		{
			if (!State.respawnTimer.Expired(Runner))
				return;

			Player player = collisionInfo.gameObject.GetComponent<Player>();
			if (!player)
				return;

			int powerup = State.activePowerupIndex;
			player.RaiseEvent( new Player.PickupEvent { powerup = powerup} );

			SetNextPowerup();
		}

		private void InitPowerupVisuals()
		{
			PowerupElement powerup = GetPowerup(State.activePowerupIndex);
			_renderer.transform.localScale = Vector3.zero;
			_meshFilter.mesh = powerup.powerupSpawnerMesh;
			_rechargeCircle.material.color = GetPowerupColor(powerup.weaponInstallationType);
		}

		private void SetNextPowerup()
		{
			State.respawnTimer = TickTimer.CreateFromSeconds(Runner, RESPAWN_TIME);
			GetPowerup(0); // Force load of powerups
			// Strictly speaking, this isn't correct since it will assign different values on proxies.
			// However, we rely on this being updated from StateAuth before the respawnTimer expires. 
			State.activePowerupIndex = Random.Range(0, _powerupElements.Length);
		}

		private Color GetPowerupColor(WeaponManager.WeaponInstallationType weaponType)
		{
			switch (weaponType)
			{
				default:
				case WeaponManager.WeaponInstallationType.PRIMARY: return _mainPowerupColor;
				case WeaponManager.WeaponInstallationType.SECONDARY: return _specialPowerupColor;
				case WeaponManager.WeaponInstallationType.BUFF: return _buffPowerupColor;
			}
		}
		
		private static PowerupElement[] _powerupElements;
		public static PowerupElement GetPowerup(int powerupIndex)
		{
			if (_powerupElements == null)
			{
				_powerupElements = Resources.LoadAll<PowerupElement>("PowerupElements");
			}
			return _powerupElements[powerupIndex];
		} 
	}
}