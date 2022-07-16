using System;
using Raylib_cs;
using System.Numerics;
using MetroidvaniaRuntime;

namespace MapEditor
{
    public static partial class GUI
    {
        public class Button : IGuiElement, IClickable
        {
            //Data
            public int X { get; set; }
            public int Y { get; set; }

            public int Width { get; set; }
            public int Height { get; set; }

            public string ButtonText { get; set; }
            public int FontSize => FontManager.FontSize;

            public int TextX => X * Screen.pixelScale + Width - ((int)Raylib.MeasureTextEx(FontManager.editorFont, ButtonText, FontSize, 0).X / 2);
            public int TextY => Y + Height + 2 - ((int)Raylib.MeasureTextEx(FontManager.editorFont, ButtonText, FontSize, 0).Y / 2);

            public ButtonState State { get; set; } = ButtonState.Normal;

            //IGuiElement
            public void DrawToWindow()
            {
                Raylib.DrawRectangle(X * Screen.pixelScale, Y * Screen.pixelScale, Width * Screen.pixelScale, Height * Screen.pixelScale, GUIColors.GetButtonColor(State));
                Raylib.DrawRectangleLines(X * Screen.pixelScale, Y * Screen.pixelScale, Width * Screen.pixelScale, Height * Screen.pixelScale, GUIColors.outline);
                Raylib.DrawTextEx(FontManager.editorFont, ButtonText, new Vector2(TextX, TextY), FontSize, 0, GUIColors.Shade(20));
            }

            //IClickable
            public event EventHandler Pressed;
            protected virtual void OnPressed(EventArgs e)
            {
                EventHandler handler = Pressed;
                handler?.Invoke(this, e);
            }
            public void Click()
            {
                OnPressed(EventArgs.Empty);
            }

            public bool IsMouseOver(Vector2 mousePos)
            {
                return !(mousePos.X < X || mousePos.Y < Y || mousePos.X > X + Width || mousePos.Y > Y + Height);
            }

            public Button(int x, int y, int width, int height, string text)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                ButtonText = text;
            }
        }

        public enum ButtonState
        {
            Normal,
            Hovered,
            Pressed,
            Selected,
        }
    }
}