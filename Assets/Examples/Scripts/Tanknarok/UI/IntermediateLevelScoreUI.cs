using System.Collections;
using FusionExamples.Utility;
using FusionHelpers;
using UnityEngine;
using TMPro;

namespace FusionExamples.Tanknarok
{
	public class IntermediateLevelScoreUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _score;
		[SerializeField] private AudioEmitter _audioEmitter;

		private bool _active;

		public void Initialize(Player player)
		{
			Color scoreColor = player.playerMaterial.GetColor("_SilhouetteColor");
			_score.color = scoreColor;
		}

		public void SetScore(int score, bool animate)
		{
			_score.enabled = true;

			if(animate)
				StartCoroutine(IncreaseScoreSequence(score,1f));
			else
				_score.text = score.ToString();
		}

		private IEnumerator IncreaseScoreSequence(int score,float switchDelay)
		{
			_active = true;
			_score.text = (score - 1).ToString();

			yield return new WaitForSeconds(switchDelay);

			_score.text = score.ToString();
			_score.transform.localScale = Vector3.one * 2f;

			_audioEmitter.PlayOneShot();

			yield return new WaitForSeconds(1f);

			_active = false;
		}

		private void Update()
		{
			if (!_active)
				return;

			_score.transform.localScale = Vector3.Lerp(_score.transform.localScale, Vector3.one, Time.deltaTime * 3f);
		}
	}
}