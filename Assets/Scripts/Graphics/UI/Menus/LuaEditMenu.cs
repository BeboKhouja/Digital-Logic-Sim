using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLS.Game;
using Newtonsoft.Json.Linq;
using Seb.Helpers;
using Seb.Types;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
	public static class LuaEditMenu
	{
        static int RowCount;

		static UIHandle ID_scrollbar;
	
		static int focusedRowIndex;
		static List<UIHandle> IDS_inputRow = new();
		static List<string> rowNumberStrings = new();

		static SubChipInstance luaChip;
		static JObject luaScript;
		static readonly UI.ScrollViewDrawElementFunc scrollViewDrawElementFunc = DrawScrollEntry;
		static readonly Func<string, bool> inputStringValidator = ProcessInput;
		static Bounds2D scrollViewBounds;
		static int lineNumberPadLength;

		static float textPad => 0.52f;
		static float height => 2.5f;

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();

			// ---- Draw ROM contents ----
			scrollViewBounds = Bounds2D.CreateFromCentreAndSize(UI.Centre, new Vector2(UI.Width * 0.4f, UI.Height * 0.8f));

			ScrollViewTheme scrollTheme = DrawSettings.ActiveUITheme.ScrollTheme;
			UI.DrawScrollView(ID_scrollbar, scrollViewBounds.TopLeft, scrollViewBounds.Size, 0, Anchor.TopLeft, scrollTheme, scrollViewDrawElementFunc, RowCount);


			if (focusedRowIndex >= 0)
			{
				// Focus next/prev field with keyboard shortcuts
				bool changeLine = KeyboardShortcuts.ConfirmShortcutTriggered || InputHelper.IsKeyDownThisFrame(KeyCode.Tab);

				if (changeLine)
				{
					bool goPrevLine = InputHelper.ShiftIsHeld;
					int jumpToRowIndex = focusedRowIndex + (goPrevLine ? -1 : 1);
					if (!goPrevLine && IDS_inputRow.Count == jumpToRowIndex) NewLine();

					if (jumpToRowIndex >= 0 && jumpToRowIndex < RowCount)
					{
						OnFieldLostFocus(focusedRowIndex);
						int nextFocusedRowIndex = focusedRowIndex + (goPrevLine ? -1 : 1);
						UI.GetInputFieldState(IDS_inputRow[nextFocusedRowIndex]).SetFocus(true);
						focusedRowIndex = nextFocusedRowIndex;
					}
				} 
			}

			// --- Draw side panel with buttons ----
			Vector2 sidePanelSize = new(UI.Width * 0.2f, UI.Height * 0.8f);
			Vector2 sidePanelTopLeft = scrollViewBounds.TopRight + Vector2.right * (UI.Width * 0.05f);
			Draw.ID sidePanelID = UI.ReservePanel();

			using (UI.BeginBoundsScope(true))
			{
				const float buttonSpacing = 0.75f;

				Vector2 buttonTopleft = new(sidePanelTopLeft.x, UI.PrevBounds.Top);

				int copyPasteButtonIndex = MenuHelper.DrawButtonPair("COPY ALL", "PASTE ALL", buttonTopleft, sidePanelSize.x, false);
				buttonTopleft = UI.PrevBounds.BottomLeft + Vector2.down * buttonSpacing;
				bool clearAll = UI.Button("CLEAR ALL", MenuHelper.Theme.ButtonTheme, buttonTopleft, new Vector2(sidePanelSize.x, 0), true, false, true, Anchor.TopLeft);
				buttonTopleft = UI.PrevBounds.BottomLeft + Vector2.down * (buttonSpacing * 2f);
				MenuHelper.CancelConfirmResult result = MenuHelper.DrawCancelConfirmButtons(buttonTopleft, sidePanelSize.x, false, false);

				MenuHelper.DrawReservedMenuPanel(sidePanelID, UI.GetCurrentBoundsScope());

				// ---- Handle button inputs ----
				if (copyPasteButtonIndex == 0) CopyAll();
				else if (copyPasteButtonIndex == 1) PasteAll();
				else if (clearAll) ClearAll();

				if (result == MenuHelper.CancelConfirmResult.Cancel || KeyboardShortcuts.CancelShortcutTriggered)
				{
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
				else if (result == MenuHelper.CancelConfirmResult.Confirm)
				{
					SaveChangesToROM();
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
			}
		}

		static void OnFieldLostFocus(int rowIndex)
		{
			if (rowIndex < 0) return;
			if (IDS_inputRow.Count < rowIndex) return;

			InputFieldState inputFieldOld = UI.GetInputFieldState(IDS_inputRow[rowIndex]);
			inputFieldOld.SetText(inputFieldOld.text, focus: false);
		}

		
		static void CopyAll()
		{
			StringBuilder sb = new();
			for (int i = 0; i < IDS_inputRow.Count; i++)
			{
				InputFieldState state = UI.GetInputFieldState(IDS_inputRow[i]);
				sb.AppendLine(state.text);
			}

			InputHelper.CopyToClipboard(sb.ToString());
		}

		static void PasteAll()
		{
			string[] pasteStrings = StringHelper.SplitByLine(InputHelper.GetClipboardContents());
			for (int i = 0; i < pasteStrings.Length; i++)
			{
				string pasteString = pasteStrings[i];
				if (IDS_inputRow.Count < pasteStrings.Length) 
					for (int ii = 0; i < (pasteStrings.Length - IDS_inputRow.Count); ii++)
						NewLine();
				RowCount = i;
				InputFieldState state = UI.GetInputFieldState(IDS_inputRow[i]);
				state.SetText(pasteString, state.focused);
			}
		}

		static void ClearAll()
		{
			InputFieldState state = UI.GetInputFieldState(IDS_inputRow[0]);
			state.SetText("", state.focused);
			IDS_inputRow.Clear();
			NewLine();
			RowCount = 1;
			focusedRowIndex = 0;
			lineNumberPadLength = 0;
		}
		static int GetDigits(int value) {
			return (int)Math.Floor(Math.Log10(value) + 1);
		}
		static void NewLine() {
			int inputRowLength = IDS_inputRow.Count + 1;
			IDS_inputRow.Add(new UIHandle("ROM_rowInputField", inputRowLength));
			InputFieldState state = UI.GetInputFieldState(IDS_inputRow.Last());
			
			state.SetText("", inputRowLength == focusedRowIndex);

			rowNumberStrings.Add((inputRowLength + ":").PadLeft(lineNumberPadLength + 1, '0'));
			if (GetDigits(inputRowLength) < GetDigits(inputRowLength + 1)) lineNumberPadLength++;
			RowCount++;
		}
		
		// Convert from uint to display string with given display mode
		static char UIntToChar(uint raw)
		{
			return Convert.ToChar(raw);
		}

		// Convert string with given format to uint
		static uint CharToUInt(char displayString)
		{
			return displayString;
		}

		
		static void SaveChangesToROM()
		{
			JObject script = JObject.Parse(luaScript.ToString());
			StringBuilder scriptBuilder = new();
			for (int i = 0; i < RowCount; i++)
			{
				string displayString = UI.GetInputFieldState(IDS_inputRow[i]).text;
				if (i != RowCount) scriptBuilder.Append(displayString + '\n');
			}
            script["script"] = scriptBuilder.ToString();
			string scriptString = script.ToString();
			int scriptLength = scriptString.Length;
			char[] scriptArray = scriptString.ToCharArray();
			for (int i = 0; i < scriptLength; i++) 
			{
				luaChip.InternalData[i] = scriptArray[i];
				if (scriptArray[i] == '\0') break;
			}
			Project.ActiveProject.NotifyLuaContentsEdited(luaChip);
		}

		static void DrawScrollEntry(Vector2 topLeft, float width, int index, bool isLayoutPass)
		{
			Vector2 panelSize = new(width, height);
			Bounds2D entryBounds = Bounds2D.CreateFromTopLeftAndSize(topLeft, panelSize);

			if (entryBounds.Overlaps(scrollViewBounds) && !isLayoutPass) // don't bother with draw stuff if outside of scroll view / in layout pass
			{
				UIHandle inputFieldID = IDS_inputRow[index];
				InputFieldState inputFieldState = UI.GetInputFieldState(inputFieldID);

				// Alternating colour for each row
				Color col = index % 2 == 0 ? ColHelper.MakeCol(0.17f) : ColHelper.MakeCol(0.13f);
				// Highlight row if it has focus
				if (inputFieldState.focused)
				{
					if (focusedRowIndex != index)
					{
						OnFieldLostFocus(focusedRowIndex);
						focusedRowIndex = index;
					}

					col = new Color(0.33f, 0.55f, 0.34f);
				}

				InputFieldTheme inputTheme = MenuHelper.Theme.ChipNameInputField;
				inputTheme.fontSize = MenuHelper.Theme.FontSizeRegular;
				inputTheme.bgCol = col;
				inputTheme.focusBorderCol = Color.clear;


				UI.InputField(inputFieldID, inputTheme, topLeft, panelSize, "", Anchor.TopLeft, 5);

				// Draw line index
				Color lineNumCol = inputFieldState.focused ? new Color(0.53f, 0.8f, 0.57f) : ColHelper.MakeCol(0.32f);
				UI.DrawText(rowNumberStrings[index], MenuHelper.Theme.FontBold, MenuHelper.Theme.FontSizeRegular, entryBounds.CentreLeft + Vector2.right * textPad, Anchor.TextCentreLeft, lineNumCol);
			}

			// Set bounding box of scroll list element 
			UI.OverridePreviousBounds(entryBounds);
		}

		static bool ProcessInput(string text) {
			// TODO: Add input processor
			return true; // Meant as a processor
		}

		public static void OnMenuOpened()
		{
			luaChip = (SubChipInstance)ContextMenu.interactionContext;
			StringBuilder sb = new();
			if (luaChip.InternalData[0] != 0) 
			{
				for (int i = 0; i < luaChip.InternalData.Length; i++)
				{
					sb.Append(UIntToChar(luaChip.InternalData[i]));
					if (UIntToChar(luaChip.InternalData[i]) == '\0') break;
				}
				luaScript = JObject.Parse(sb.ToString());
			} 
			else 
			{
				luaScript = new();
				luaScript["script"] = "";
			}
			string script = (string)luaScript["script"];
			string[] scriptLines = StringHelper.SplitByLine(script);
			RowCount = scriptLines.Length;
						
			ID_scrollbar = new UIHandle("ROM_EditScrollbar", luaChip.ID);
			
			focusedRowIndex = 0;
			
			lineNumberPadLength = RowCount.ToString().Length;

			for (int i = 0; i < scriptLines.Length; i++)
			{
				IDS_inputRow.Add(new UIHandle("ROM_rowInputField", i));
				InputFieldState state = UI.GetInputFieldState(IDS_inputRow.Last());
			
				state.SetText(scriptLines[i], i == focusedRowIndex);

				rowNumberStrings.Add((i + 1 + ":").PadLeft(lineNumberPadLength + 1, '0'));
			}

        }

		public static void Reset()
		{
			//dataDisplayModeIndex = 0;
		}

	}
}