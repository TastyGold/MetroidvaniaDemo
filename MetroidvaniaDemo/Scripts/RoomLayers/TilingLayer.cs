using System;
using System.IO;
using UndoRedo;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public class TilingLayer : AbstractLayer
        {
            //Layer data
            public int WorldPosX => parentRoom.RoomGlobalPosX;
            public int WorldPosY => parentRoom.RoomGlobalPosY;
            public bool isSolid;
            public byte textureId = 1;

            //Tiles
            public byte[,] tiles;

            public void SetAllTiles(byte[,] tileIDs)
            {
                this.tiles = tileIDs;
            }

            public override void ResizeLayer(int deltaX, int deltaY, int deltaWidth, int deltaHeight)
            {
                byte[,] newTiles = new byte[tiles.GetLength(0) + deltaWidth, tiles.GetLength(1) + deltaHeight];
                for (int y = 0; y < newTiles.GetLength(1); y++)
                {
                    for (int x = 0; x < newTiles.GetLength(0); x++)
                    {
                        if (!(x + deltaX < 0 || x + deltaX >= tiles.GetLength(0) || y + deltaY < 0 || y + deltaY >= tiles.GetLength(1)))
                        {
                            newTiles[x, y] = tiles[x + deltaX, y + deltaY];
                        }
                    }
                }
                tiles = newTiles;
            }

            //SaveLoad
            public override byte GetLayerTypeId() => 0;

            public override void WriteToBinaryFile(BinaryWriter bin)
            {
                base.WriteToBinaryFile(bin);
                bin.Write(textureId);
                for (int y = 0; y < parentRoom.RoomHeight; y++)
                {
                    for (int x = 0; x < parentRoom.RoomWidth; x++)
                    {
                        bin.Write(tiles[x, y]);
                    }
                }
            }
            public static AbstractLayer ReadLayerFromBinaryFile(BinaryReader bin, Room parentRoom)
            {
                TilingLayer layerData = new TilingLayer(parentRoom);
                layerData.textureId = bin.ReadByte();
                for (int y = 0; y < parentRoom.RoomHeight; y++)
                {
                    for (int x = 0; x < parentRoom.RoomWidth; x++)
                    {
                        layerData.tiles[x, y] = bin.ReadByte();
                    }
                }
                return layerData;
            }

            public override void InitialiseTextures()
            {
                renderer.texture = parentRoom.TexManager.RequestTexture("..//..//..//maintilesheet.png");
            }

            //Renderer
            private readonly TilingRenderer renderer = new TilingRenderer();
            public override void DrawAll()
            {
                renderer.Render(this);
            }

            //UndoRedo
            public byte[,] GetSavestate()
            {
                int lx = tiles.GetLength(0), ly = tiles.GetLength(1);
                byte[,] newTiles = new byte[lx, ly];
                for (int x = 0; x < lx; x++)
                {
                    for (int y = 0; y < ly; y++)
                    {
                        newTiles[x, y] = tiles[x, y];
                    }
                }
                return newTiles;
            }
            public void ApplySavestate(byte[,] savestate)
            {
                int lx = savestate.GetLength(0), ly = savestate.GetLength(1);
                tiles = new byte[lx, ly];
                for (int x = 0; x < lx; x++)
                {
                    for (int y = 0; y < ly; y++)
                    {
                        tiles[x, y] = savestate[x, y];
                    }
                }
            }
            public class TileChangeAction : IAction
            {
                private readonly TilingLayer tiling;
                public byte[,] before;
                public byte[,] after;

                public void Execute()
                {
                    tiling.ApplySavestate(after);
                }
                public void Unexecute()
                {
                    tiling.ApplySavestate(before);
                }

                public TileChangeAction(TilingLayer layer)
                {
                    tiling = layer;
                }
            }

            //Constructors
            public TilingLayer(Room parent) : base(parent)
            {
                tiles = new byte[parent.RoomWidth, parent.RoomHeight];
            }
        }
    }
}