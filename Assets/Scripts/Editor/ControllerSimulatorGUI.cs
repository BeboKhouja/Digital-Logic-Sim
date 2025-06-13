using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;

namespace DLS.Editor {
    public class ControllerSimulatorGUI : EditorWindow
    {
        [MenuItem("Window/General/Controller Simulator")]
        public static void ShowGUI()
        {
            ControllerSimulatorGUI wnd = GetWindow<ControllerSimulatorGUI>();
            wnd.titleContent = new GUIContent("Controller Simulator");
        }


        private static readonly Dictionary<string, GamepadButton> controllerButtons = new() {
            {"A", GamepadButton.A},
            {"B", GamepadButton.B},
            {"X", GamepadButton.X},
            {"Y", GamepadButton.Y},
            {"LS", GamepadButton.LeftShoulder},
            {"RS", GamepadButton.RightShoulder},
            {"LT", GamepadButton.LeftTrigger},
            {"RT", GamepadButton.RightTrigger},
            {"Select", GamepadButton.Select},
            {"Start", GamepadButton.Start},
        };

        private static readonly Dictionary<string, GamepadButton> dpadButtons = new() {
            {"Up", GamepadButton.DpadUp},
            {"Down", GamepadButton.DpadDown},
            {"Left", GamepadButton.DpadLeft},
            {"Right", GamepadButton.DpadRight},
        };

        private static readonly Dictionary<string, GamepadButton> combinedButtons = new() {
            {"A", GamepadButton.A},
            {"B", GamepadButton.B},
            {"X", GamepadButton.X},
            {"Y", GamepadButton.Y},
            {"LS", GamepadButton.LeftShoulder},
            {"RS", GamepadButton.RightShoulder},
            {"LT", GamepadButton.LeftTrigger},
            {"RT", GamepadButton.RightTrigger},
            {"Select", GamepadButton.Select},
            {"Start", GamepadButton.Start},
            {"Up", GamepadButton.DpadUp},
            {"Down", GamepadButton.DpadDown},
            {"Left", GamepadButton.DpadLeft},
            {"Right", GamepadButton.DpadRight},
        };
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement enableController = new Toggle("Enable Controller");
            enableController.name = "enableController";
            root.Add(enableController);

            AddButtons();

            SetupButtons();
        }

        public void AddButtons() {
            VisualElement root = rootVisualElement;
            ToggleButtonGroup buttons = new ToggleButtonGroup {
                allowEmptySelection = true,
                isMultipleSelection = true,
                name = "buttons",
            };
            ToggleButtonGroup dpadButtons = new ToggleButtonGroup {
                allowEmptySelection = true,
                name = "dpadButtons",
            };

            foreach(var pair in controllerButtons) {
                Button button = new() {text = pair.Key, name = pair.Key};
                buttons.Add(button);
            }

            foreach(var pair in ControllerSimulatorGUI.dpadButtons) {
                Button button = new() {text = pair.Key, name = pair.Key};
                dpadButtons.Add(button);
            }

            buttons.SetEnabled(false);
            dpadButtons.SetEnabled(false);

            root.Add(buttons);
            root.Add(dpadButtons);
        }

        public void Update() {
            VisualElement root = rootVisualElement;
            VisualElement elements = root.Query();
            elements.SetEnabled(EditorApplication.isPlaying);
            Debug.Log(Gamepad.current?.aButton.isPressed);
        }

        private void SetupButtons() {
            VisualElement root = rootVisualElement;
            Toggle enableController = root.Q<Toggle>("enableController");
            enableController.RegisterCallback<ClickEvent>(_ => {
                root.Query<ToggleButtonGroup>().ForEach(e => e.SetEnabled(enableController.value));
                if (enableController.value) 
                    InputSystem.AddDevice<Gamepad>().MakeCurrent();
                else 
                    InputSystem.RemoveDevice(Gamepad.current);
            });

            root.Query<ToggleButtonGroup>().ForEach(group => 
                group.Query<Button>().ForEach(e => 
                    e.RegisterCallback<ClickEvent>(_ => {
                        unsafe {
                            Gamepad.current[combinedButtons.First(button => button.Key == e.name).Value].stateBlock
                                .WriteFloat((void*)0, GetToggleButtonStateFromName(group, e.name) ? 0 : 1);
                        }
                    })
                )
            );
        }
        private static bool GetToggleButtonStateFromName(ToggleButtonGroup group, string name) {
            VisualElement[] buttons = group.Children().ToArray();
            VisualElement button = buttons.First(e => e.name == name);
            int index = IndexOf(group.Children(), button);
            return group.value[index];
        }

        private static int IndexOf<T>(IEnumerable<T> source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }
    }
}
