using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace FusionHelpers
{
	public interface INetworkEvent : INetworkStruct
	{
	}
	
	/// <summary>
	/// The TickAlignedEventRelay is a networked object that gives each peer in shared mode a list
	/// of outgoing events meant for the State Authority of other peers.
	/// 
	/// Each peer will process all lists for all other peers looking for events destined for itself,
	/// and then execute that event.
	/// 
	/// The primary reason to use this over a regular RPC is that it allow synchronous execution of
	/// state changes for multiple State Authorities.
	/// 
	/// For example, in Tanknarok, when peer A fires a bullet on peer B, it is A that detects the collision,
	/// removes the bullet, triggers an explosion FX and decreases ammo count if relevant.
	/// 
	/// However, A cannot alter the visual state of B or reduce its HP since it does not have
	/// StateAuthority over it, so instead it sends an event to tell B to do it.
	/// 
	/// Because the event is part of A's state, it arrives at B in the same tick as the bullet
	/// destruction and the explosion FX, and everything will occur simultaneously as seen from B's perspective.
	/// (Note however that B will probably not be in the same state it was when A registered the event)
	/// 
	/// This is not a silver bullet, and does not generally replace RPCs. Specifically, the
	/// need to pre-allocate the event structure at build-time means there are certain limitations:
	/// 
	/// * You need to have a reasonable limit on the number of events that may be sent per tick
	/// * The event structure uses memory based on the largest possible event you have, so need to keep event size down, or use multiple event relays.
	/// * There's a risk of loosing events because the buffer is cyclic and will re-use slots as soon as they have been sent.
	/// </summary>

	public class TickAlignedEventRelay: NetworkBehaviour
	{
		// Theoretical maximum number of events you'll ever send with each relay in a single tick.
		// In reality this needs to be a couple of times larger than that to avoid loosing events due to package drops.
		const int MAX_EVENTS = 10;
		
		// The maximum size of any event sent with the relay (in bytes).
		const int MAX_EVENT_SIZE = 24;

		// Each event has a header which identifies the event and its intended target authority as well as a byte array payload.
		private struct EventHeader : INetworkStruct
		{
			public int id { get; set; }
			public int type { get; set; }
			public NetworkId target { get; set; }
		}

		[Networked, Capacity(MAX_EVENTS)] private NetworkArray<EventHeader> _eventHeaders => default;
		[Networked, Capacity(MAX_EVENTS*MAX_EVENT_SIZE)] private NetworkArray<byte> _eventBuffer => default;

		private int _nextEventIndex = 1;
		private int _handledEventIndex;

		private unsafe delegate void ITypeWrapper(int typeIndex, byte* data);
		private List<Type> _registeredTypes = new();
		private List<ITypeWrapper> _listeners = new();

		/// <summary>
		/// Register an event listener for a specific type of event. When you call this method a map of event type and type IDs
		/// is built dynamically, so it is crucial that calls to this method for any given relay is always done in the same order
		/// on all peers.
		///
		/// Preferably, call this only from Spawned() of a single NB, and register all your listeners unconditionally.
		///
		/// Note that the callback will trigger on both the source of the event (immediately for predictive updates),
		/// as well as on State Authority and proxies. Deal with that how you like.
		/// </summary>
		/// <param name="listener">The callback that will receive the event</param>
		/// <typeparam name="T">Type of event struct</typeparam>

		public void RegisterEventListener<T>(Action<T> listener) where T:unmanaged, INetworkEvent
		{
			int monitoredTypeIndex = _registeredTypes.IndexOf(typeof(T));
			if (monitoredTypeIndex < 0)
			{
				monitoredTypeIndex = _registeredTypes.Count;
				_registeredTypes.Add(typeof(T));
			}

			unsafe
			{
				_listeners.Add((int typeIndex, byte* data) =>
				{
					if (typeIndex == monitoredTypeIndex)
					{
						listener(*(T*) data);
					}
				});
			}
		}

		/// <summary>
		/// Send event to be executed on the State Authority of another peer.
		///
		/// This will trigger on the local peer immediately, regardless of whether it is StateAuthority or not.
		/// </summary>
		/// <param name="target">A relay owned by the target StateAuthority (May be *this*, but generally isn't) </param>
		/// <param name="evt">The event struct to send</param>
		/// <typeparam name="T">The type of the event struct</typeparam>
		public void RaiseEventFor<T>(TickAlignedEventRelay target, T evt) where T:unmanaged, INetworkEvent
		{
			unsafe
			{
				Assert.Check( sizeof(T)<MAX_EVENT_SIZE, $"Event of type {typeof(T)} is larger ({sizeof(T)} bytes) than MAX_EVENT_SIZE ({MAX_EVENT_SIZE} bytes)");
			}
			
			byte[] bytes = SerializeValueType(evt);
			
			// Predict it locally
			int typeIndex = _registeredTypes.IndexOf(typeof(T));
			target.OnTickAlignedEvent(typeIndex, bytes);

			// Do nothing in hosted mode - we're either authority over everything or nothing at all, nothing more to do here.
			if (Runner==null || Runner.Topology != Topologies.Shared)
				return;

			// If we don't have StateAuthority we're going to have to let SA know so it can change it properly.
			if (!target.HasStateAuthority)
			{
				EventHeader head = new();
				head.target = target.Object.Id;
				head.id = _nextEventIndex;
				head.type = typeIndex;
				int index = _nextEventIndex % _eventHeaders.Length;
				_eventHeaders.Set(index, head);
				for (int i = 0; i < bytes.Length; i++)
				{
					_eventBuffer.Set(index*MAX_EVENT_SIZE+i, bytes[i]);
				}
				_nextEventIndex++;
			}
		}

		private unsafe void OnTickAlignedEvent(int typeIndex, byte[] evt)
		{
			fixed (byte* buffer = evt)
			{
				foreach (ITypeWrapper listener in _listeners)
				{
					listener(typeIndex, buffer);
				}
			}
		}

		public override void Render()
		{
			if (HasStateAuthority)
				return; // If we have State Authority then these are our outgoing messages and none of them are for us!

			if (TryGetSnapshotsBuffers(out var fromBuffer, out _, out _))
			{
				var headersReader = GetArrayReader<EventHeader>(nameof(_eventHeaders));
				var headers = headersReader.Read(fromBuffer);
				var byteReader = GetArrayReader<byte>(nameof(_eventBuffer));
				var bytes = byteReader.Read(fromBuffer);
				int handledId = _handledEventIndex;
				for(int i=0;i<headers.Length;i++)
				{
					EventHeader head = headers[i];
					if(head.id>_handledEventIndex)
					{
						handledId = Mathf.Max(handledId, head.id);
						if (Runner.TryFindObject(head.target, out NetworkObject no))
						{
							TickAlignedEventRelay behaviour = no.GetComponent<TickAlignedEventRelay>();
							byte[] buffer = new byte[MAX_EVENT_SIZE];
							for (int b = 0; b < buffer.Length; b++)
								buffer[b] = bytes[i * MAX_EVENT_SIZE + b];
							behaviour.OnTickAlignedEvent(head.type,buffer);
						}
					}
				}
				_handledEventIndex = handledId;
			}
		}
		
		public static unsafe byte[] SerializeValueType<T>(in T value) where T : unmanaged
		{
			byte[] result = new byte[sizeof(T)];
			fixed (byte* dst = result)
				*(T*)dst = value;
			return result;
		}
	}
}