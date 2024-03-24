using FusionHelpers;
using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public abstract class AutoReleasedFx : MonoBehaviour
	{
		private float _timeToDie;
		
		protected abstract float Duration { get; }

		protected void OnEnable()
		{
			_timeToDie = Duration;
		}

		private void Update()
		{
			if (_timeToDie > 0)
			{
				_timeToDie -= Time.deltaTime;
				if(_timeToDie<=0)
					LocalObjectPool.Release(this);
			}
		}		
	}
}