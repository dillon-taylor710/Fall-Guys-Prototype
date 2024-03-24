using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FusionExamples.Tanknarok
{
	public class MuzzleFlashGigavolt : MuzzleFlash
	{
		[FormerlySerializedAs("lines")] [SerializeField] private LineRenderer[] _lines;

		private float _squiggleSpacing = .75f; // How far apart should the points be added on the lines, in units of distance.

		[FormerlySerializedAs("squiggleIntensityCurve")] [SerializeField] private AnimationCurve _squiggleIntensityCurve = null;
		[FormerlySerializedAs("fadeCurve")] [SerializeField] private AnimationCurve _fadeCurve = null;
		[FormerlySerializedAs("fadeDuration")] [SerializeField] private float _fadeDuration = 1f;

		public override void OnFire(ShotState shot)
		{
			base.OnFire(shot);
			Debug.Log($"Fire Zappy from {shot.Position} to {shot.Position+shot.Direction}");
			DrawAllBeams(shot.Position, shot.Position+shot.Direction);
		}

		private void DrawAllBeams(Vector3 origin, Vector3 target)
		{
			float distance = Vector3.Distance(origin, target);
			for (int i = 0; i < _lines.Length; i++)
			{
				LineRenderer line = _lines[i];
				line.enabled = true;

				if (i == 0)
				{
					// draw the first beam as a thick straight line
					DrawBeam(line, origin, target, distance, false);
				}
				else
				{
					// draw the rest of the beams as squiggled lines
					DrawBeam(line, origin, target, distance, true);
				}

				// start fading out the beam
				StartCoroutine(FadeBeam(line, distance));
			}
		}

		// Draw a squiggely beam between fromPosition and toPosition, and add the distance too because this function is too lazy to figure it out.
		private void DrawBeam(LineRenderer lr, Vector3 fromPosition, Vector3 toPosition, float distance, bool squiggled)
		{
			List<Vector3> points = new List<Vector3>();
			//add the first point, which shouldn't be randomized in any way
			points.Add(fromPosition);

			// only if the line is squiggled do we need to add points inbetween from and to position
			if (squiggled)
			{
				int numPoints = Mathf.FloorToInt(distance);

				// add all the in-between points to the list of points.
				float pointDistance = _squiggleSpacing;
				while (pointDistance < distance)
				{
					//The squigglecurve determines how randomized each point is - a lower number leads to a more focused beam.
					Vector3 point = Vector3.MoveTowards(fromPosition, toPosition, pointDistance);
					float randomIntensity = _squiggleIntensityCurve.Evaluate(pointDistance / distance);
					Vector3 randomizedPoint = RandomizePoint(point, randomIntensity);

					points.Add(randomizedPoint);

					pointDistance += _squiggleSpacing;
				}
			}

			// add the end point
			points.Add(toPosition);

			// set all the points in the line renderer
			lr.positionCount = points.Count;
			lr.SetPositions(points.ToArray());
		}

		// Add some randomization to a given Vector3
		private Vector3 RandomizePoint(Vector3 point, float intensity)
		{
			float x = point.x + Random.Range(intensity * -1, intensity);
			float y = point.y + Random.Range(intensity * -1, intensity);
			float z = point.z + Random.Range(intensity * -1, intensity);

			return new Vector3(x, y, z);
		}

		private IEnumerator FadeBeam(LineRenderer lr, float distance)
		{
			float timer = 0;
			float duration = _fadeDuration;

			while (timer < duration)
			{
				float t = timer / duration;

				lr.material.SetFloat("_BeamFade", _fadeCurve.Evaluate(t));

				timer += Time.deltaTime;
				yield return null;
			}

			lr.material.SetFloat("_BeamFade", 1);
		}
	}
}