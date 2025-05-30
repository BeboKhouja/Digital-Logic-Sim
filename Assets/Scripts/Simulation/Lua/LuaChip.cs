using System.Linq;
using DLS.Description;
using DLS.Game;
using NLua.Exceptions;
using UnityEngine;
using System.Collections.Generic;

namespace DLS.Simulation.Lua {
    public class PinState {
        public static readonly uint High = Simulation.PinState.LogicHigh;
        public static readonly uint Low = Simulation.PinState.LogicLow;
        public static readonly uint Disconnected = Simulation.PinState.LogicDisconnected;
    }
    public class Chip {
        private SimChip chip;
        private SubChipInstance subChip;
        public Chip(SimChip chip) {
            this.chip = chip;
            Project.ActiveProject.ViewedChip.TryGetSubChipByID(this.chip.ID, out subChip);
        }
        public void AddPin(object name, int bitCount, int id, bool input) {
            ValidatePinIDs(subChip.AllPins.ToArray());
            PinDescription pinDescription = new(name.ToString(), id, Vector2.zero, PinBitCountFromInt(bitCount), PinColour.Red, PinValueDisplayMode.Off);
            PinInstance pinInstance = new(pinDescription, new(chip.ID, id), subChip, !input);
            subChip.AllPins.Add(pinInstance);
            subChip.ReevaluatePinout();
            ReevaluateSize();
            subChip.UpdatePinLayout();
            chip.AddPin(new(id, input, chip), input);
        }
        public uint GetPinState(int index) {
            return chip.InputPins[index].State;
        }
        public void SetPinState(int index, uint val) {
            chip.OutputPins[index].State = val;
        }
        private void ReevaluateSize() {
            Vector2 size = new(
                BuiltinChipCreator.CalculateGridSnappedWidth(Graphics.DrawSettings.GridSize * 9),
                SubChipInstance.MinChipHeightForPins(
                    subChip.InputPins.Select(e => e.Description).ToArray(),
                    subChip.OutputPins.Select(e => e.Description).ToArray()
                )
            );
            subChip.Size = size;
            subChip.Description.Size = size;
        }
        private static PinBitCount PinBitCountFromInt(int val) {
            switch (val) {
                case 1:
                    return PinBitCount.Bit1;
                case 4:
                    return PinBitCount.Bit4;
                case 8:
                    return PinBitCount.Bit8;
                default:
                    throw new LuaException("Pin bit count outside range");
            };
        }
        private static void ValidatePinIDs(PinInstance[] pins)
		{
			HashSet<int> pinIDs = new();

			if (pins == null) return;
			foreach (PinInstance pin in pins)
			{
				if (!pinIDs.Add(pin.Address.PinID))
				{
					throw new LuaException($"Pin has duplicate ID ({pin.Address.PinID}) in custom Lua chip");
				}
			}
		}
    }
}