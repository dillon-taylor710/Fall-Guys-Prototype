using UnityEngine;

namespace FusionHelpers
{
	public class LocalObjectPool : ObjectPool<MonoBehaviour>
	{
		public static LocalObjectPool _localPool;

		public static T Acquire<T>(T prefab, Vector3 pos = default, Quaternion rot = default, Transform p = default) where T : MonoBehaviour
		{
			if (_localPool == null)
			{
				GameObject go = new GameObject("LocalObjectPool");
				DontDestroyOnLoad(go);
				_localPool = go.AddComponent<LocalObjectPool>();
			}
			return (T) _localPool.AcquireInstance(prefab, pos,rot,p);
		}

		public static void Release(MonoBehaviour obj)
		{
			_localPool.ReleaseInstance(obj);
		}
	}
}