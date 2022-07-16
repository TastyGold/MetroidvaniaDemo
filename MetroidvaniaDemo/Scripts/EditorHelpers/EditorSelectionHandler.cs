using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using MetroidvaniaLevels;

namespace MapEditor
{
    public class EditorSelectionHandler
    {
        //Config
        private readonly Color selectionColorA = new Color(200, 200, 200, 50);

        //Data
        public Level currentLevel;
        public Room currentRoom;

        public List<IEditorSelectable> selection = new List<IEditorSelectable>();

        //Mouse positin
        public int mouseOriginX;
        public int mouseOriginY;
        public int mouseX, mouseY;

        //Methods
        public void DrawSelectionBox()
        {
            DrawSelectionBox(mouseOriginX, mouseOriginY, mouseX, mouseY);
        }
        public void DrawSelectionBox(int mx1, int my1, int mx2, int my2)
        {
            Raylib.DrawLine(mx1, my1, mx2, my1, selectionColorA);
            Raylib.DrawLine(mx1, my1, mx1, my2, selectionColorA);
            Raylib.DrawLine(mx2, my1, mx2, my2, selectionColorA);
            Raylib.DrawLine(mx1, my2, mx2, my2, selectionColorA);
        }
    }
}