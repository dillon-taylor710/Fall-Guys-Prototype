using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace FusionHelpers
{
	/// <summary>
	/// <p>Simple extension for the NetworkRunner that allow registration of singletons as well as waiting for a singleton of a specific type to become available.</p>
	/// <ul>
	/// <li>To register a singleton, call Runner.RegisterSingleton(this) from Spawned() (or Awake if not a network object).</li>
	/// </ul>
	/// <p>Note that you can provide a higher-level type if you wish to register a single instance as the singleton for an entire hierarchy of types.</p>
	/// <ul>
	/// <li>To acquire a singleton, call Runner.GetSingleton&lt;Type&gt;() - don't cache this as it may change if the singleton is re-created, for example during host migration.</li>
	/// <li>To wait for a singleton, call Runner.WaitForSingleton&lt;Type&gt;()</li>
	/// </ul>
	/// </summary>
	public static class FusionRunnerSingletons
	{
		private static Dictionary<NetworkRunner, Dictionary<Type,object>> _singletonsByRunner = new Dictionary<NetworkRunner, Dictionary<Type, object>>();
		private static Dictionary<NetworkRunner, Dictionary<Type,List<Action<object>>>> _pendingRequestsByRunner = new Dictionary<NetworkRunner, Dictionary<Type, List<Action<object>>>>();

		/// <summary>
		/// Get a singleton of a specific type. Will return false if none is available.
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="singleton">Placeholder for requested singleton</param>
		/// <typeparam name="T">Type of singleton to get</typeparam>
		/// <returns>false if none is available</returns>
		public static bool TryGetSingleton<T>(this NetworkRunner runner, out T singleton) where T : MonoBehaviour
		{
			if (_singletonsByRunner.TryGetValue(runner, out var singletonsByType))
			{
				if (singletonsByType.TryGetValue(typeof(T), out var s))
				{
					if ((T)s)
					{
						singleton = (T) s;
						return true;
					}
					singletonsByType.Remove(typeof(T));
				}
				if (singletonsByType.Count == 0)
					_singletonsByRunner.Remove(runner);
			}
			singleton = null;
			return false;
		}

		/// <summary>
		/// Wait for a singleton of a specific type to be registered. 
		/// </summary> 
		/// <param name="runner"></param>
		/// <param name="onSingletonResolved">Callback that will be invoked with the singleton instance once it registers</param>
		/// <typeparam name="T">Type of singleton to wait for</typeparam>
		/// <returns>Handle that can be passed to StopWaitingForSingleton if you need to cancel the pending request</returns>
		public static Action<object> WaitForSingleton<T>(this NetworkRunner runner, Action<T> onSingletonResolved) where T : MonoBehaviour
		{
			if (runner.TryGetSingleton(out T singleton))
			{
				onSingletonResolved(singleton);
				return null;
			}

			if (!_pendingRequestsByRunner.TryGetValue(runner, out Dictionary<Type,List<Action<object>>> pendingByType))
			{
				pendingByType = new Dictionary<Type, List<Action<object>>>();
				_pendingRequestsByRunner[runner] = pendingByType;
			}

			if (!pendingByType.TryGetValue(typeof(T), out List<Action<object>> pendingRequests))
			{
				pendingRequests = new List<Action<object>>();
				pendingByType[typeof(T)] = pendingRequests;
			}

			Action<object> untypedCallback = resolvedSingleton => { onSingletonResolved((T) resolvedSingleton); };
			pendingRequests.Add( untypedCallback );

			Debug.Log($"Waiting for singleton of type {typeof(T)}");
			return untypedCallback;
		}

		/// <summary>
		/// Cancel a pending request for a singleton.
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="handler">The handler that was previously registered (return value from WaitForSingleton)</param>
		public static void StopWaitingForSingleton(this NetworkRunner runner, Action<object> handler)
		{
			if (_pendingRequestsByRunner.TryGetValue(runner, out Dictionary<Type, List<Action<object>>> pendingByType))
			{
				foreach (KeyValuePair<Type,List<Action<object>>> keyValuePair in pendingByType)
				{
					if (keyValuePair.Value.Remove(handler))
					{
						if (keyValuePair.Value.Count == 0)
							pendingByType.Remove(keyValuePair.Key);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Remove all registered singletons and pending requests for singletons on a specific runner.
		/// Should be called when the runner shuts down.
		/// </summary>
		/// <param name="runner"></param>
		public static void ClearRunnerSingletons(this NetworkRunner runner)
		{
			_pendingRequestsByRunner.Remove(runner);
			_singletonsByRunner.Remove(runner);
		}

		/// <summary>
		/// Unregister singleton for all the types that it represents
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="singleton">The singleton instance to unregister</param>
		/// <typeparam name="T">Actual type of the singleton</typeparam>
		public static void UnregisterSingleton<T>(this NetworkRunner runner, T singleton) where T: MonoBehaviour
		{
			if (_singletonsByRunner.TryGetValue(runner, out var singletonsByType))
			{
				Type type = typeof(T);
				while (typeof(MonoBehaviour).IsAssignableFrom(type))
				{
					if (singletonsByType.TryGetValue(type, out var registeredSingleton))
					{
						if (registeredSingleton == singleton)
						{
							Debug.Log($"Un-registering singleton {singleton} of type {type}");
							singletonsByType.Remove(type);
						}
					}
					type = type.BaseType;
				}
			}
		}

		/// <summary>
		/// Register an object instance as the singleton for its type.
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="singleton">The singleton</param>
		/// <typeparam name="T">The only type for which the singleton is explicitly registered</typeparam>
		public static void RegisterSingleton<T>(this NetworkRunner runner, T singleton) where T: MonoBehaviour
		{
			Debug.Log($"Registering singleton of type {typeof(T)}");
			if (!_singletonsByRunner.TryGetValue(runner, out var singletonsByType))
			{
				singletonsByType = new Dictionary<Type, object>();
				_singletonsByRunner[runner] = singletonsByType;
			}

			Type type = typeof(T);
			RegisterSingleton(runner, singletonsByType, type, singleton);
		}

		/// <summary>
		/// Register an object instance as the singleton for its explicit type and all parent types up to and including the provided base-type.
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="basetype">The top-most type that this singleton represents</param>
		/// <param name="singleton">The singleton</param>
		/// <typeparam name="T">The specific type of the singleton</typeparam>
		public static void RegisterSingleton<T>(this NetworkRunner runner, Type basetype, T singleton) where T: MonoBehaviour
		{
			Debug.Log($"Registering singleton of type {typeof(T)} for basetype {basetype}");
			if (!_singletonsByRunner.TryGetValue(runner, out var singletonsByType))
			{
				singletonsByType = new Dictionary<Type, object>();
				_singletonsByRunner[runner] = singletonsByType;
			}

			Type type = typeof(T);
			while (type!=null && basetype.IsAssignableFrom(type))
			{
				Debug.Log($"  Registering derived type {type} for base {basetype}");
				RegisterSingleton(runner, singletonsByType, type, singleton);
				type = type.BaseType;
			}
		}

		private static void RegisterSingleton<T>(NetworkRunner runner, Dictionary<Type,object> singletonsByType, Type singletonType, T singletonInstance) where T: MonoBehaviour
		{
			if (singletonsByType.TryGetValue(singletonType, out _))
				throw new Exception($"Attempt to register {typeof(T)} twice as a singleton for the same runner!");

			singletonsByType[singletonType] = singletonInstance;

			if (_pendingRequestsByRunner.TryGetValue(runner, out var pendingRequestsByType)) 
			{
				Debug.Log("Resolving pending requests for Runner!");
				if (pendingRequestsByType.TryGetValue(singletonType, out List<Action<object>> pendingRequests))
				{
					foreach (var pendingRequest in pendingRequests) 
					{
						pendingRequest(singletonInstance);
					}
					pendingRequests.Clear();
					pendingRequestsByType.Remove(singletonType);
					if (pendingRequestsByType.Count == 0)
						_pendingRequestsByRunner.Remove(runner);
				}
			}
		}
	}
}