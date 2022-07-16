using System;
using System.Numerics;

namespace MapEditor
{
    public static partial class GUI
    {
        public interface IClickable
        {
            public bool IsMouseOver(Vector2 mousePos);
            public event EventHandler Pressed;
        }
    }
}