using System.Collections.Generic;
using System.Linq;
using DLS.Description;
using Seb.Types;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics {
    public class ControllerUI {
        public static List<ControllerButtonDescription> buttons {get; private set;} = new();
        public static Vector2 StartFrom = UI.BottomLeft + Vector2.right + Vector2.up * 6;

        public static void DrawUI() {
            
            DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			const int buttonPad = 1;
			Color buttonLabelCol = Color.white;
			Vector2 labelPosCurr = StartFrom;

			using (UI.BeginBoundsScope(true))
			{
                foreach (ControllerButtonDescription button in buttons) {
                    UI.DrawText(button.Name, theme.FontBold, theme.FontSizeRegular, labelPosCurr, Anchor.TextCentreLeft, buttonLabelCol);
                }
            }
        }

        public static void SetButtons(ControllerButtonDescription[] buttons) => ControllerUI.buttons = buttons.ToList();
        public static void SetButtons(List<ControllerButtonDescription> buttons) => ControllerUI.buttons = buttons;
        public static void Reset() {
            buttons.Clear();
        }
    }
}