using Fusion;

namespace FusionHelpers
{
	/// <summary>
	/// Baseclass for Network Behaviours that follow a pattern of keeping all networked properties in its own struct.
	/// This reduces the amount of boilerplate needed to manage snapshots and change detection, at the cost of some flexibility.
	/// For example, you can check for changes and get both previous and current states with a single call, but you won't know exactly which properties changed.
	/// You can also acquire previous and current snapshots without dealing with property readers.
	/// Networked data is accessed via the `State` property which can be a useful visual hint when reading (and writing) code.
	/// (Note that while you *can* add networked state outside of the provided struct,
	/// it is not recommended since it most likely leads to extra calls to change detectors and is just generally confusing)
	/// </summary>
	/// <typeparam name="T">The struct that declares the networked state of this behaviour</typeparam>

	public abstract class NetworkBehaviourWithState<T> : NetworkBehaviour where T : unmanaged, INetworkStruct
	{
		public abstract ref T State { get; }

		private ChangeDetector _changesSimulation;
		private ChangeDetector _changesFrom;
		private ChangeDetector _changesTo;

		protected bool TryGetStateChanges(out T previous, out T current, ChangeDetector.Source source = ChangeDetector.Source.SimulationState)
		{
			switch (source)
			{
				default:
				case ChangeDetector.Source.SimulationState:
					return TryGetStateChanges(source, ref _changesSimulation, out previous, out current);
				case ChangeDetector.Source.SnapshotFrom:
					return TryGetStateChanges(source, ref _changesFrom, out previous, out current);
				case ChangeDetector.Source.SnapshotTo:
					return TryGetStateChanges(source, ref _changesTo, out previous, out current);
			}
		}

		private bool TryGetStateChanges(ChangeDetector.Source source, ref ChangeDetector changes, out T previous, out T current)
		{
			if(changes==null)
				changes = GetChangeDetector(source);

			if (changes != null)
			{
				foreach (var change in changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
				{
					switch (change)
					{
						case nameof(State):
							var reader = GetPropertyReader<T>(change);
							current = currentBuffer.Read(reader);
							previous = previousBuffer.Read(reader);
							return true;
					}
				}
			}
			current = default;
			previous = default;
			return false;
		}

		protected bool TryGetStateSnapshots(out T from, out Tick fromTick, out T to, out Tick toTick, out float alpha)
		{
			if (TryGetSnapshotsBuffers(out var fromBuffer, out var toBuffer, out alpha))
			{
				var reader = GetPropertyReader<T>(nameof(State));
				from = fromBuffer.Read(reader);
				to = toBuffer.Read(reader);
				fromTick = fromBuffer.Tick;
				toTick = toBuffer.Tick;
				return true;
			}

			from = default;
			to = default;
			fromTick = default;
			toTick = default;
			return false;
		}
	}
}