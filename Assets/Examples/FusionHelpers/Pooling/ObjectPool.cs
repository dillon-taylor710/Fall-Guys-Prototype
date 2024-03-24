using System.Collections.Generic;
using UnityEngine;

namespace FusionHelpers
{
	/// <summary>
	/// Generic Object Pool. This class keeps a pool for each distinct prefab and keeps track of which pool created instances belong to.
	/// </summary>
	public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
	{
		private Dictionary<T, ObjectList> _poolsByPrefab = new Dictionary<T, ObjectList>();
		private Dictionary<T, ObjectList> _poolsByInstance = new Dictionary<T, ObjectList>();

		private ObjectList GetPool(T prefab) 
		{
			ObjectList pool;
			if (!_poolsByPrefab.TryGetValue(prefab, out pool))
			{
				pool = new ObjectList();
				_poolsByPrefab[prefab] = pool;
			}

			return pool;
		}

		public T AcquireInstance(T prefab, Vector3 pos = default, Quaternion rot = default, Transform parent = default)
		{
			ObjectList pool = GetPool(prefab);
			T newt = pool.GetFromPool(Vector3.zero, Quaternion.identity);

			if (newt == null)
			{
				Debug.Log($"Creating new instance for prefab {prefab}");
				newt = Instantiate(prefab, pos, rot, parent);
				_poolsByInstance[newt] = pool;
			}
			else
			{
				newt.transform.SetParent(parent, false);
				newt.transform.position = pos;
				newt.transform.rotation = rot;
			}

			newt.gameObject.SetActive(true);
			return newt;
		}

		public void ReleaseInstance(T oldt)
		{
			if (oldt)
			{
				ObjectList pool;
				if (_poolsByInstance.TryGetValue(oldt, out pool))
				{
					pool.ReturnToPool(oldt);
					oldt.gameObject.SetActive(false); // Should always disable before re-parenting, or we will dirty it twice
					oldt.transform.SetParent(transform, false);
				}
				else
				{
					Destroy(oldt.gameObject);
				}
			}
		}

		public void ClearPools()
		{
			foreach (ObjectList pool in _poolsByPrefab.Values)
			{
				pool.Clear();
			}

			foreach (ObjectList pool in _poolsByInstance.Values)
			{
				pool.Clear();
			}

			_poolsByPrefab.Clear();
			_poolsByInstance.Clear();
		}

		private class ObjectList
		{
			private List<T> _free = new List<T>();

			public T GetFromPool(Vector3 p, Quaternion q, Transform parent = null)
			{
				T newt = null;

				while (_free.Count > 0 && newt==null)
				{
					var t = _free[0];
					if (t) // In case a recycled object was destroyed
					{
						Transform xform = t.transform;
						xform.SetParent(parent, false);
						xform.position = p;
						xform.rotation = q;
						newt = t;
					}
					else
					{
						Debug.LogWarning("Recycled object was destroyed - not re-using!");
					}

					_free.RemoveAt(0);
				}

				return newt;
			}

			public void Clear()
			{
				foreach (var pooled in _free)
				{
					if (pooled)
					{
						Debug.Log($"Destroying pooled object: {pooled.gameObject.name}");
						Destroy(pooled.gameObject);
					}
				}

				_free.Clear();
			}

			public void ReturnToPool(T no)
			{
				_free.Add(no);
			}
		}
	}
}