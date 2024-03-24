using Fusion;
using FusionHelpers;
using TMPro;
using UnityEngine;

public class DebugUI : NetworkBehaviour
{
	[SerializeField] private TMP_Text _text;

	void Update()
	{
		if (Runner)
			Runner.WaitForSingleton<FusionSession>(session => { _text.text = $"Me:{Runner.LocalPlayer} Tick:{Runner.Tick}"; });
	}
}
