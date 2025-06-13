using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DLS.Description {
    public enum ButtonType {
        A,
        B,
        X,
        Y,
        Select,
        Start,
        DPad_Up,
        DPad_Down,
        DPad_Left,
        DPad_Right,
        LeftStick,
        RightStick,
        LeftShoulder,
        RightShoulder,
        LeftTrigger,
        RightTrigger,
    }
    public class ControllerButtonDescription {
        public readonly ButtonType Button;
        public readonly string Name;
        public ControllerButtonDescription(ButtonType button, string name) {
            Button = button;
            Name = name;
        }
        public static ButtonControl GetButtonControlFromButtonType(Gamepad gamepad, ButtonType button) =>
            button switch {
                ButtonType.A => gamepad.aButton,
                ButtonType.B => gamepad.bButton,
                ButtonType.X => gamepad.xButton,
                ButtonType.Y => gamepad.yButton,
                ButtonType.Select => gamepad.selectButton,
                ButtonType.Start => gamepad.startButton,
                ButtonType.DPad_Up => gamepad.dpad.up,
                ButtonType.DPad_Down => gamepad.dpad.down,
                ButtonType.DPad_Left => gamepad.dpad.left,
                ButtonType.DPad_Right => gamepad.dpad.right,
                ButtonType.LeftShoulder => gamepad.leftShoulder,
                ButtonType.RightShoulder => gamepad.rightShoulder,
                ButtonType.LeftTrigger => gamepad.leftTrigger,
                ButtonType.RightTrigger => gamepad.rightTrigger,
                _ => throw new NotImplementedException()
            };
    }
}