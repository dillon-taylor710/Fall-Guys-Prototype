using System;
using Fusion;
using UnityEngine;

namespace FusionHelpers
{
	/// <summary>
	/// Example of a Fusion Object Pool.
	/// </summary>

	public class PooledNetworkObjectProvider : NetworkObjectProviderDefault
	{
		[SerializeField]
		private NetworkObjectPool _pool;

		protected void Awake()
		{
			_pool = gameObject.AddComponent<NetworkObjectPool>();
		}

		protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
		{
			return _pool.AcquireInstance(prefab);
		}

		protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
		{
			_pool.ReleaseInstance(instance);
		}
	}
}