using Raylib_cs;
using System.Numerics;
using MetroidHelper;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public class TilingRenderer
        {
            public Texture2D texture;//Raylib.LoadTexture("..//..//..//maintilesheet.png");

            public Rectangle GetTileSourceRec(byte index) //0x00: empty, 0xAB - A= spritesheet column, B= spritesheet row
            {
                return new Rectangle((index & 0x0f) * 16, (index >> 4) * 16, 16, 16).Debleed();
            }

            public void Render(TilingLayer layer)
            {
                for (int x = 0; x < layer.SizeX; x++)
                {
                    for (int y = 0; y < layer.SizeY; y++)
                    {
                        int scaleMultiplier = 16 * MetroidvaniaRuntime.Screen.pixelScale; //subject to change with screen scaling (!)
                        Vector2 position = new Vector2(layer.WorldPosX + x, layer.WorldPosY + y) * scaleMultiplier;
                        Rectangle destRec = new Rectangle(position.X, position.Y, scaleMultiplier, scaleMultiplier);
                        Raylib.DrawTexturePro(texture, GetTileSourceRec(layer.tiles[x, y]), destRec, Vector2.Zero, 0, Color.WHITE);
                    }
                }
            }
        }
    }
}