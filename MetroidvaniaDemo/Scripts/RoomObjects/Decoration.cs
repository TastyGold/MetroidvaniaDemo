using MathExtras;
using System.IO;
using MapEditor;
using Raylib_cs;
using System.Numerics;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public class Decoration : IEditorSelectable
        {
            //Position data in room
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            //TextureID is the number which will be assigned by the renderer representing the texture atlas
            public byte AtlasIndex { get; set; }
            //SpriteID is the name of the sprite on the CSV file for the texture atlas
            public string SpriteID { get; set; }

            //Temp data
            public int width, height;

            //Selection
            public bool DoesOverlapSelection(Rectangle rect)
            {
                return !(X + width < rect.x || X > rect.x + rect.width || Y + height < rect.y || Y > rect.y + rect.height);
            }
            public void DragTranslate(int mouseDeltaX, int mouseDeltaY)
            {
                X += mouseDeltaX;
                Y += mouseDeltaY;
            }
            public string GetObjectName()
            {
                return SpriteID;
            }
            public Rectangle GetBoundingBox()
            {
                return new Rectangle(X, Y, width, height);
            }

            //Constructor
            public Decoration(int x, int y, byte atlasId, string spriteId, int z = 0)
            {
                X = x;
                Y = y;
                Z = z;
                AtlasIndex = atlasId;
                SpriteID = spriteId;
            }
        }
    }
}