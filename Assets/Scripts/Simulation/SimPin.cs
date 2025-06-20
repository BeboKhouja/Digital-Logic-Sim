using System;
using System.Collections;
using DLS.Description;

namespace DLS.Simulation
{
	public class SimPin
	{
		public readonly int ID;
		public readonly SimChip parentChip;
		public readonly bool isInput;
		public PinStateValue State;

		public SimPin[] ConnectedTargetPins = Array.Empty<SimPin>();

		// Simulation frame index on which pin last received an input
		public int lastUpdatedFrameIndex;

		// Address of pin from where this pin last received its input
		public int latestSourceID;
		public int latestSourceParentChipID;

		// Number of wires that input their signal to this pin.
		// (In the case of conflicting signals, the pin chooses randomly)
		public int numInputConnections;
		public int numInputsReceivedThisFrame;

		public SimPin(int id, bool isInput, SimChip parentChip, PinBitCount pinBitCount)
		{
			this.parentChip = parentChip;
			this.isInput = isInput;
			ID = id;
			latestSourceID = -1;
			latestSourceParentChipID = -1;

			State.MakeFromPinBitCount(pinBitCount);
			State.SetAllDisconnected();
		}

		public bool FirstBitHigh => State.FirstBitHigh();

		public void PropagateSignal()
		{
			int length = ConnectedTargetPins.Length;
			for (int i = 0; i < length; i++)
			{
				ConnectedTargetPins[i].ReceiveInput(this);
			}
		}

		// Called on sub-chip input pins, or chip dev-pins
		void ReceiveInput(SimPin source)
		{
			// If this is the first input of the frame, reset the received inputs counter to zero
			if (lastUpdatedFrameIndex != Simulator.simulationFrame)
			{
				lastUpdatedFrameIndex = Simulator.simulationFrame;
				numInputsReceivedThisFrame = 0;
			}

			bool set;

			if (numInputsReceivedThisFrame > 0)
			{
				set = State.HandleConflicts(source.State);
			}
			else
			{
				// First input source this frame, so accept it.
				State = source.State;
				set = true;
			}

			if (set)
			{
				latestSourceID = source.ID;
				latestSourceParentChipID = source.parentChip.ID;
			}

			numInputsReceivedThisFrame++;

			// If this is a sub-chip input pin, and has received all of its connections, notify the sub-chip that the input is ready
			if (isInput && numInputsReceivedThisFrame == numInputConnections)
			{
				parentChip.numInputsReady++;
			}
		}
	}
}