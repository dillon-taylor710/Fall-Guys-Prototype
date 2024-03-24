using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public class ForceField : MonoBehaviour
	{
		[SerializeField] Material forceFieldMaterial;

		[SerializeField] MeshRenderer meshRenderer;

		string[] toggleProperties = new string[4] {"_PLAYER1Toggle", "_PLAYER2Toggle", "_PLAYER3Toggle", "_PLAYER4Toggle"};
		string[] positionProperties = new string[4] {"_PositionPLAYER1", "_PositionPLAYER2", "_PositionPLAYER3", "_PositionPLAYER4"};

		void Awake()
		{
			forceFieldMaterial = new Material(forceFieldMaterial);
			meshRenderer.material = forceFieldMaterial;
		}

		public void SetPlayer(int index, MonoBehaviour behaviour)
		{
			if (behaviour != null)
				forceFieldMaterial.SetVector(positionProperties[index], behaviour.transform.position);
			forceFieldMaterial.SetInt(toggleProperties[index], behaviour==null ? 0 : 1);
		}
	}
}