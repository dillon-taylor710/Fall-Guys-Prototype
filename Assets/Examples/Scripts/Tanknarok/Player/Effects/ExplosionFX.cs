using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public class ExplosionFX : AutoReleasedFx
	{
		[SerializeField] private AudioEmitter _audioEmitter;
		[SerializeField] private ParticleSystem _particle;

		protected override float Duration => _particle ? _particle.main.duration : 2.0f;
		
		private void OnValidate()
		{
			if (!_audioEmitter)
				_audioEmitter = GetComponent<AudioEmitter>();
			if (!_particle)
				_particle = GetComponent<ParticleSystem>();
		}

		private new void OnEnable()
		{
			base.OnEnable();
			if (_audioEmitter)
				_audioEmitter.PlayOneShot();
			if (_particle)
				_particle.Play();
		}

		private void OnDisable()
		{
			if (_particle)
			{
				_particle.Stop();
				_particle.Clear();
			}
		}
	}
}