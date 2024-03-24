using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public class TankTeleportOutEffect : AutoReleasedFx
	{
		[SerializeField] private float _duration = 5.0f;
		[SerializeField] private Transform _dummyTankTurret;
		[SerializeField] private Transform _dummyTankHull;

		[SerializeField] private ParticleSystem _teleportEffect;

		[Header("Audio")] 
		[SerializeField] private AudioEmitter _audioEmitter;

		protected override float Duration => _duration;
		
		public void StartTeleport(Color color, Quaternion turretRotation, Quaternion hullRotation)
		{
			ColorChanger.ChangeColor(transform, color);
			
			_teleportEffect.Stop();
			
			if(_audioEmitter.isActiveAndEnabled)
				_audioEmitter.PlayOneShot();

			_dummyTankTurret.rotation = turretRotation;
			_dummyTankHull.rotation = hullRotation;

			_teleportEffect.Play();
		}
	}
}