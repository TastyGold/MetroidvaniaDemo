using Raylib_cs;

namespace MapEditor
{
    public static partial class GUI
    {
        public static class GUIColors
        {
            public static Color Shade(int value) => new Color(value, value, value, 255);

            public static Color normal = Shade(220);
            public static Color hovered = Shade(210);
            public static Color pressed = Shade(190);
            public static Color selected = new Color(193, 202, 247, 255);
            public static Color outline = Shade(190);

            public static Color GetButtonColor(ButtonState state) => state switch
            {
                ButtonState.Normal => normal,
                ButtonState.Hovered => hovered,
                ButtonState.Pressed => pressed,
                ButtonState.Selected => selected,
                _ => throw new System.Exception()
            };
        }
    }
}