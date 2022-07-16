using Raylib_cs;
using System.Numerics;

namespace MapEditor
{
    public interface IEditorSelectable
    {
        public bool DoesOverlapSelection(Rectangle rect);
        public void DragTranslate(int mouseDeltaX, int mouseDeltaY);
        public string GetObjectName();
        public Rectangle GetBoundingBox();
    }
}