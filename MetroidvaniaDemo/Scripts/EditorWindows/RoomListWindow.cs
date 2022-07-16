using System;
using System.Collections.Generic;
using Raylib_cs;
using MetroidvaniaRuntime;
using System.Numerics;
using static MathExtras.MathHelper;

namespace MapEditor
{
    public class RoomListWindow : BaseWindow, IBehavedWindow
    {
        public EditorSelectionHandler selectionHandler;
        private MetroidvaniaLevels.Level EditorLevel => selectionHandler.currentLevel;
        private MetroidvaniaLevels.Room SelectedRoom => selectionHandler.currentRoom;

        private readonly string[] headers = new string[5] { "Room", "X", "Y", "Width", "Height" };
        private readonly int defaultColumnWidth = 40;
        private int[] columnWidths;
        private int FontSize => FontManager.FontSize;
        private readonly int PaddingSize = 3 * Screen.pixelScale;
        private int RowSize => FontSize + (PaddingSize * 2);
        private Font TextFont => FontManager.editorFont;
        private readonly Color TextColor = Color.BLACK;

        private readonly int scrollSpeed = 8;
        private int scrollValue = 0;
        private int listLength;

        private int GetColumnX(int column) => columnWidths.PartialSum(column) * Screen.pixelScale;
        private Vector2 GetTextPosition(int column, int row)
        {
            // row -1 is headers, and doesn't scroll
            return new Vector2(PaddingSize + GetColumnX(column), PaddingSize + (row != -1 ? RowSize * (row + 1) + scrollValue : 0));
        }

        private void UpdateColumnWidths(int fontSize)
        {
            int n = (int)(defaultColumnWidth * ((float)fontSize / 11)) / Screen.pixelScale;
            columnWidths = new int[5] { n, n, n, n, n };
        }

        private void DrawHeaders()
        {
            Raylib.DrawRectangle(0, 0, windowWidth, RowSize, Color.RAYWHITE);
            for (int i = 0; i < headers.Length; i++)
            {
                Raylib.DrawTextEx(TextFont, headers[i], GetTextPosition(i, -1), FontSize, 0, TextColor);
                if (i != 0) Raylib.DrawLine(GetColumnX(i), 0, GetColumnX(i), EditorManager.screenHeight, Color.LIGHTGRAY);
            }
            Raylib.DrawLine(0, RowSize, windowWidth, RowSize, Color.LIGHTGRAY);
        }
        private void DrawRoomData()
        {
            int roomNum = 0;
            foreach (KeyValuePair<string, MetroidvaniaLevels.Room> pair in EditorLevel.RoomDictionary)
            {
                int textHeight = RowSize * (roomNum + 1) + scrollValue + PaddingSize;
                int lineHeight = RowSize * (roomNum + 2) + scrollValue;
                if (pair.Value == SelectedRoom) Raylib.DrawRectangle(0, lineHeight - (RowSize - 1), windowWidth, RowSize - 1, new Color(220, 220, 220, 255));

                Raylib.DrawTextEx(TextFont, pair.Key, GetTextPosition(0, roomNum), FontSize, 0, TextColor);
                Raylib.DrawTextEx(TextFont, pair.Value.RoomGlobalPosX.ToString(), GetTextPosition(1, roomNum), FontSize, 0, TextColor);
                Raylib.DrawTextEx(TextFont, pair.Value.RoomGlobalPosY.ToString(), GetTextPosition(2, roomNum), FontSize, 0, TextColor);
                Raylib.DrawTextEx(TextFont, pair.Value.RoomWidth.ToString(), GetTextPosition(3, roomNum), FontSize, 0, TextColor);
                Raylib.DrawTextEx(TextFont, pair.Value.RoomHeight.ToString(), GetTextPosition(4, roomNum), FontSize, 0, TextColor);

                Raylib.DrawLine(0, lineHeight, windowWidth, lineHeight, Color.LIGHTGRAY);
                roomNum++;
            }
            listLength = roomNum;
        }
        private void HandleMouseScrolling()
        {
            UpdateMouseState();
            if (isMouseOver)
            {
                int scrollAmount = (int)Raylib.GetMouseWheelMove();
                scrollValue += scrollAmount * scrollSpeed;
                scrollValue = Math.Clamp(scrollValue, (listLength - 1) * -32, 0);
            }
        }

        public void RunWindowBehaviour()
        {
            HandleMouseScrolling();

            BeginDrawing();
            ClearBackground();

            DrawRoomData();
            DrawHeaders();

            EndDrawing();
            DrawToScreen();
        }

        public RoomListWindow(EditorSelectionHandler selectionHandler)
        {
            int lWidth = EditorManager.leftMenuWidth;
            windowScreenX = 0;
            windowScreenY = 0;
            windowWidth = lWidth;
            windowHeight = EditorManager.screenHeight;
            windowBackgroundColor = Color.RAYWHITE;
            renderTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
            this.selectionHandler = selectionHandler;
            FontManager.OnFontReload += UpdateColumnWidths;
            UpdateColumnWidths(FontManager.FontSize);
        }
    }
}