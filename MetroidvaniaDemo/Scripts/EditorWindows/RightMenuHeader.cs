using InputHelper;
using MetroidvaniaRuntime;
using Raylib_cs;
using System;
using System.Numerics;

namespace MapEditor
{
    public class RightMenuHeader : BaseWindow, IBehavedWindow
    {
        private readonly string[] buttonTexts = { "Tiling", "Deco", "Select" };
        private GUI.Button[] buttons;
        private readonly int buttonWidth = 44;

        private void InitialiseButtons()
        {
            buttons = new GUI.Button[buttonTexts.Length];
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                buttons[i] = new GUI.Button(4 + i * (buttonWidth + 4), 2, buttonWidth, windowHeight / Screen.pixelScale - 4, buttonTexts[i]);
            }
            buttons[0].Pressed += Tiling_Pressed;
            buttons[1].Pressed += Deco_Pressed;
            buttons[2].Pressed += Select_Pressed;
        }

        private void Tiling_Pressed(object sender, EventArgs e)
        {
            EditorManager.RWindowMode = EditorManager.WindowMode.TileSelect;
        }
        private void Deco_Pressed(object sender, EventArgs e)
        {
            EditorManager.RWindowMode = EditorManager.WindowMode.DecoSelect;
        }
        private void Select_Pressed(object sender, EventArgs e)
        {
            EditorManager.RWindowMode = EditorManager.WindowMode.Selection;
        }

        public void RunWindowBehaviour()
        {
            BeginDrawing();
            ClearBackground();

            HandleButtons();

            EndDrawing();
            DrawToScreen();
        }

        private void HandleButtons()
        {
            UpdateMouseState();
            foreach (GUI.Button b in buttons)
            {
                if (b.State != GUI.ButtonState.Selected)
                {
                    if (b.IsMouseOver(GetMouseWindowPos()))
                    {
                        if (Input.Released_LMB && b.State == GUI.ButtonState.Pressed)
                        {
                            b.Click();
                            b.State = GUI.ButtonState.Selected;
                            foreach (GUI.Button button in buttons)
                            {
                                if (button != b)
                                {
                                    button.State = GUI.ButtonState.Normal;
                                }
                            }
                        }
                        else if (Input.Held_LMB)
                        {
                            b.State = GUI.ButtonState.Pressed;
                        }
                        else
                        {
                            b.State = GUI.ButtonState.Hovered;
                        }
                    }
                    else
                    {
                        b.State = GUI.ButtonState.Normal;
                    }
                }
                b.DrawToWindow();
            }
        }

        public void SetMode(EditorManager.WindowMode mode)
        {
            GUI.Button b = mode switch
            {
                EditorManager.WindowMode.TileSelect => buttons[0],
                EditorManager.WindowMode.DecoSelect => buttons[1],
                EditorManager.WindowMode.Selection => buttons[2],
            };
            b.Click();
            b.State = GUI.ButtonState.Selected;
            foreach (GUI.Button button in buttons)
            {
                if (button != b)
                {
                    button.State = GUI.ButtonState.Normal;
                }
            }
        }

        public Vector2 GetMouseWindowPos()
        {
            return (mouseCurrentPosition - new Vector2(windowScreenX, windowScreenY)) / Screen.pixelScale;
        }

        public RightMenuHeader()
        {
            windowScreenX = EditorManager.screenWidth - EditorManager.rightMenuWidth;
            windowWidth = EditorManager.rightMenuWidth;
            windowHeight = EditorManager.rightHeaderHeight;
            windowBackgroundColor = Color.RAYWHITE;
            renderTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
            InitialiseButtons();
        }
    }
}