using Raylib_cs;
using System;
using MapEditor;
using InputHelper;
using MetroidvaniaLevels;
using TextureAtlases;
using System.Diagnostics;

namespace MetroidvaniaRuntime
{
    static class EntryPoint
    {
        public static void Main()
        {
            bool editor = true;

            if (editor)
            {
                EditorManager.Main_Editor();
            }
        }
    }

    public static class Screen
    {
        public static int pixelScale = 2;
    }

    public static class EditorManager
    {
        public const int ratioWidth = 1256;
        public const int normalWidth = 800;
        public static int screenWidth = ratioWidth * Screen.pixelScale;
        public static int screenHeight = 450 * Screen.pixelScale;
        public static int leftMenuWidth = 200 * Screen.pixelScale;
        public static int rightMenuWidth = 256 * Screen.pixelScale;
        public static int rightHeaderHeight = 20 * Screen.pixelScale;
        public static EditorWindow editorWindow;
        public static TileSelectWindow tsWindow;
        public static RoomListWindow roomListWindow;
        public static DecoSelectWindow dsWindow;
        public static RightMenuHeader rightHeader;
        public static WindowMode RWindowMode = WindowMode.DecoSelect;

        public enum WindowMode
        {
            TileSelect,
            DecoSelect,
            Selection
        }

        public static void Main_Editor()
        {
            //Initialisation
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);
            Raylib.SetTargetFPS(120);
            Raylib.InitWindow(screenWidth, screenHeight, "Level Editor - Testlevel.bin");
            Raylib.SetWindowMinSize(leftMenuWidth + rightMenuWidth, 400);

            EditorSelectionHandler selectionHandler = new EditorSelectionHandler();
            Level mainLevel;
            if (!System.IO.File.Exists(Directories.Maps + "Testlevel.bin"))
            {
                mainLevel = new Level();
                mainLevel.AddRoom("a_01", 0, 0, 22, 18);
                mainLevel.AddRoom("a_02", 22, 0, 22, 18);
            }
            else mainLevel = FileSaveLoad.ReadLevelFromFile("Testlevel.bin");
            selectionHandler.currentLevel = mainLevel;
            selectionHandler.currentRoom = mainLevel.RoomDictionary[mainLevel.startingRoom];

            tsWindow = new TileSelectWindow();
            dsWindow = new DecoSelectWindow();
            editorWindow = new EditorWindow(selectionHandler) { tileSelector = tsWindow, decoSelector = dsWindow };
            roomListWindow = new RoomListWindow(selectionHandler);
            rightHeader = new RightMenuHeader(); 
            rightHeader.SetMode(RWindowMode);

            //Main update loop
            while (!Raylib.WindowShouldClose())
            {
                //Window resizing
                if (Raylib.IsWindowResized()) ResizeLayout();
                if (Input.Held_LCTRL)
                {
                    if (Input.Held_LSHIFT)
                    {
                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
                        {
                            FontManager.ReloadFont(FontManager.FontSize + Screen.pixelScale);
                        }
                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
                        {
                            FontManager.ReloadFont(FontManager.FontSize - Screen.pixelScale);
                        }
                    }
                    else
                    {
                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                        {
                            FileSaveLoad.SaveLevelToFile("Testlevel.bin", mainLevel);
                        }
                    }
                }
                else
                {
                    WindowMode m = RWindowMode;
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_T)) RWindowMode = WindowMode.TileSelect;
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_D)) RWindowMode = WindowMode.DecoSelect;
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_S)) RWindowMode = WindowMode.Selection;
                    if (m != RWindowMode) rightHeader.SetMode(RWindowMode);
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.RAYWHITE);
                switch (RWindowMode)
                {
                    case WindowMode.TileSelect:
                        tsWindow.RunWindowBehaviour();
                        break;
                    case WindowMode.DecoSelect:
                        dsWindow.RunWindowBehaviour();
                        break;
                    default:
                        break;
                }
                editorWindow.RunWindowBehaviour();
                roomListWindow.RunWindowBehaviour();
                rightHeader.RunWindowBehaviour();
                Raylib.EndDrawing();
            }

            //Closing
            Raylib.CloseWindow();
        }

        public static void ResizeLayout()
        {
            screenWidth = Raylib.GetScreenWidth();
            screenHeight = Raylib.GetScreenHeight();
            Console.WriteLine($"Changed window size: <{screenWidth}, {screenHeight}>");

            int width = screenWidth;
            int height = screenHeight;

            editorWindow.Resize(width - leftMenuWidth - rightMenuWidth, height);
            tsWindow.windowScreenX = screenWidth - rightMenuWidth;
            tsWindow.windowScreenY = rightHeaderHeight;
            dsWindow.windowScreenX = tsWindow.windowScreenX;
            dsWindow.windowScreenY = rightHeaderHeight;
            rightHeader.windowScreenX = tsWindow.windowScreenX;
            roomListWindow.Resize(leftMenuWidth, screenHeight);
        }
    }
}