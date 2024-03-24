using UnityEngine;
using Fusion;

namespace FusionExamples.Tanknarok
{
	public class RotatingTurret : NetworkBehaviour
	{
		[SerializeField] private LaserBeam[] _laserBeams;
		[SerializeField] private float _rpm;
		[SerializeField] private float _timeOffset;

		public override void Spawned()
		{
			for (int i = 0; i < _laserBeams.Length; i++)
				_laserBeams[i].Init();
		}

		public override void Render()
		{
			transform.rotation = Quaternion.Euler(0, (Object.RenderTime - _timeOffset) * _rpm, 0);
			for (int i = 0; i < _laserBeams.Length; i++)
				_laserBeams[i].Render( Runner.IsForward );
		}
	}
}