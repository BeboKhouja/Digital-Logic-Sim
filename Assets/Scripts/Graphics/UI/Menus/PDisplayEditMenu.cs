using System;
using System.Collections.Generic;
using System.Linq;
using DLS.Game;
using DLS.SaveSystem;
using Seb.Helpers;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
	public static class PDisplayEditMenu
	{
		static SubChipInstance pDisplayChip;
		static string windowTitle;

		static readonly UIHandle ID_PulseWidthInput = new("PDisplayChipEdit_WindowTitle");
		static readonly Func<string, bool> stringInputValidator = ValidateWindowTitleInput;

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();
			Draw.ID panelID = UI.ReservePanel();
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			Vector2 pos = UI.Centre + Vector2.up * (UI.HalfHeight * 0.25f);

			using (UI.BeginBoundsScope(true))
			{
				UI.DrawText("Program name", theme.FontBold, theme.FontSizeRegular, pos, Anchor.TextCentre, Color.white * 0.8f);

				InputFieldTheme inputFieldTheme = DrawSettings.ActiveUITheme.ChipNameInputField;

				Vector2 inputPos = UI.PrevBounds.CentreBottom + Vector2.down * DrawSettings.VerticalButtonSpacing;
				(Vector2 size, float pad) = GetTextInputSize();
				InputFieldState state = UI.InputField(ID_PulseWidthInput, inputFieldTheme, inputPos, size, string.Empty, Anchor.CentreTop, pad, stringInputValidator, true);

				MenuHelper.CancelConfirmResult result = MenuHelper.DrawCancelConfirmButtons(UI.GetCurrentBoundsScope().BottomLeft, UI.GetCurrentBoundsScope().Width, true);
				MenuHelper.DrawReservedMenuPanel(panelID, UI.GetCurrentBoundsScope());

				if (result == MenuHelper.CancelConfirmResult.Cancel)
				{
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
				else if (result == MenuHelper.CancelConfirmResult.Confirm)
				{
					Project.ActiveProject.NotifyPDisplayChanged(pDisplayChip, state.text);
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
			}
		}

		public static (Vector2 size, float pad) GetTextInputSize()
		{
			const float textPad = 2;
			InputFieldTheme inputTheme = DrawSettings.ActiveUITheme.ChipNameInputField;
			Vector2 inputFieldSize = UI.CalculateTextSize(new string('M', DescriptionCreator.WindowTitleLength - 1), inputTheme.fontSize, inputTheme.font) + new Vector2(textPad * 2, 3);
			return (inputFieldSize, textPad);
		}

		public static void OnMenuOpened()
		{
			pDisplayChip = (SubChipInstance)ContextMenu.interactionContext;
			List<char> chars = new();
			for (int i = 0; i < pDisplayChip.InternalData.Length; i++)
			{
				chars.Add((char)pDisplayChip.InternalData[i]);
				if (pDisplayChip.InternalData[i] == 0) break;
			}
			windowTitle = new(chars.ToArray());
			UI.GetInputFieldState(ID_PulseWidthInput).SetText(windowTitle);
		}

		public static bool ValidateWindowTitleInput(string s)
		{
			if (s.Length >= DescriptionCreator.WindowTitleLength) return false;
			return true;
		}
	}
}