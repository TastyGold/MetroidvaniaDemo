using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using UndoRedo;
using TextureAtlases;

namespace MetroidvaniaLevels
{
    public partial class Room : IBinaryWritable
    {
        //References
        public TextureManager TexManager { get; set; }

        //Primary room class
        public string roomName;
        public int RoomGlobalPosX { get; private set; }
        public int RoomGlobalPosY { get; private set; }
        public int RoomWidth { get; private set; }
        public int RoomHeight { get; private set; }

        //Room editing
        public void Transform(int deltaX, int deltaY, int deltaWidth, int deltaHeight)
        {
            OnRoomTransform?.Invoke(deltaX, deltaY, deltaWidth, deltaHeight);
            TransformWithoutInvoke(deltaX, deltaY, deltaWidth, deltaHeight);
        }
        private void TransformWithoutInvoke(int deltaX, int deltaY, int deltaWidth, int deltaHeight)
        {
            RoomGlobalPosX += deltaX;
            RoomGlobalPosY += deltaY;
            RoomWidth += deltaWidth;
            RoomHeight += deltaHeight;
        }
        public void Translate(int deltaX, int deltaY)
        {
            RoomGlobalPosX += deltaX;
            RoomGlobalPosY += deltaY;
        }

        public delegate void RoomTransformHandler(int deltaX, int deltaY, int deltaWidth, int deltaHeight);
        public RoomTransformHandler OnRoomTransform;
        public class RoomTransformAction : IAction
        {
            private readonly Room targetRoom;
            public readonly int dx, dy, dw, dh;
            private List<object> layerSavestates;

            public void Execute()
            {
                layerSavestates = new List<object>();

                for (int i = 0; i < targetRoom.layers.Count; i++)
                {
                    switch (targetRoom.layers[i])
                    {
                        case TilingLayer tiling:
                            layerSavestates.Add(tiling.GetSavestate());
                            tiling.ResizeLayer(dx, dy, dw, dh);
                            break;
                        case DecoLayer deco:
                            layerSavestates.Add(null);
                            deco.ResizeLayer(dx, dy, dw, dh);
                            break;
                        default:
                            layerSavestates.Add(null);
                            break;
                    }
                }

                targetRoom.TransformWithoutInvoke(dx, dy, dw, dh);
            }
            public void Unexecute()
            {
                targetRoom.TransformWithoutInvoke(-dx, -dy, -dw, -dh);

                for (int i = 0; i < layerSavestates.Count; i++)
                {
                    switch (layerSavestates[i])
                    {
                        case byte[,] tiles:
                            (targetRoom.layers[i] as TilingLayer).ResizeLayer(-dx, -dy, -dw, -dh);
                            (targetRoom.layers[i] as TilingLayer).ApplySavestate(tiles);
                            break;
                        case null:
                            break;
                    }
                }

                for (int i = 0; i < targetRoom.layers.Count; i++)
                {
                    switch (targetRoom.layers[i])
                    {
                        case DecoLayer deco:
                            deco.ResizeLayer(-dx, -dy, -dw, -dh);
                            break;
                        default:
                            break;
                    }
                }
            }

            public RoomTransformAction(Room targetRoom, int deltaX, int deltaY, int deltaWitdh, int deltaHeight)
            {
                this.targetRoom = targetRoom;
                this.dx = deltaX;
                this.dy = deltaY;
                this.dw = deltaWitdh;
                this.dh = deltaHeight;
            }
        }
        public class RoomTranslateAction : IAction
        {
            private readonly Room targetRoom;
            public readonly int dx, dy;

            public void Unexecute() => targetRoom.Translate(-dx, -dy);
            public void Execute() => targetRoom.Translate(dx, dy);

            public RoomTranslateAction(Room targetRoom, int deltaX, int deltaY)
            {
                this.targetRoom = targetRoom;
                this.dx = deltaX;
                this.dy = deltaY;
            }
        }

        //Layer handling
        private List<AbstractLayer> layers = new List<AbstractLayer>();

        public void AddLayer(AbstractLayer newLayer)
        {
            layers.Add(newLayer);
            if (TexManager != null) newLayer.InitialiseTextures();
            Console.WriteLine($"Added new layer of type: {newLayer.GetLayerTypeId()}");
            ReorderLayerZ();
        }
        public void RemoveLayer(AbstractLayer al)
        {
            int index = -1;
            try
            {
                index = layers.FindIndex(m => m == al);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Ignored attempt to remove non-existent layer from list.");
            }
        }
        public AbstractLayer GetLayerOfType(int typeId, int occurence = 0)
        {
            for (int i = 0, o = occurence; i < layers.Count; i++)
            {
                if (layers[i].GetLayerTypeId() == typeId)
                {
                    if (o == 0) return layers[i];
                    else o--;
                }
            }
            throw new Exception($"Layer typeId: {typeId} #{occurence} does not exist.");
        }
        public bool HasLayerOfType(int typeId)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].GetLayerTypeId() == typeId) return true;
            }
            return false;
        }
        public int GetLayerCountOfType(int typeId)
        {
            int count = 0;
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].GetLayerTypeId() == typeId) count++;
            }
            return count;
        }

        public void ReorderLayerZ()
        {
            List<AbstractLayer> newLayers = new List<AbstractLayer>();
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].GetLayerTypeId() == 0)
                {
                    newLayers.Add(layers[i]);
                }
                else
                {
                    newLayers.Insert(0, layers[i]);
                }
            }
            layers = newLayers;
        }

        //Methods
        public void DrawInEditor(Camera2D editorCamera, bool drawGrid = true, bool selected = true)
        {
            int tileSize = 16 * MetroidvaniaRuntime.Screen.pixelScale;
            Color gridColor = MapEditor.EditorWindow.gridColor;
            int bgC = selected ? 60 : 55;

            //Background
            Raylib.DrawRectangle(RoomGlobalPosX * tileSize, RoomGlobalPosY * tileSize, RoomWidth * tileSize, RoomHeight * tileSize, new Color(bgC, bgC, bgC, 255));

            //Layers
            foreach (AbstractLayer layer in layers)
            {
                layer.DrawAll();
            }

            DrawEditorGrid(drawGrid, selected, tileSize, gridColor);
        }
        public void DrawEditorGrid(bool drawGrid, bool selected, int tileSize, Color gridColor)
        {
            //Grid
            if (selected)
            {
                for (int x = RoomGlobalPosX; x < RoomGlobalPosX + RoomWidth + 1; x += drawGrid ? 1 : RoomWidth)
                {
                    Raylib.DrawLine(x * tileSize, RoomGlobalPosY * tileSize, x * tileSize, (RoomGlobalPosY + RoomHeight) * tileSize, gridColor);
                }
                for (int y = RoomGlobalPosY; y < RoomGlobalPosY + RoomHeight + 1; y += drawGrid ? 1 : RoomHeight)
                {
                    Raylib.DrawLine(RoomGlobalPosX * tileSize, y * tileSize, (RoomGlobalPosX + RoomWidth) * tileSize, y * tileSize, gridColor);
                }
            }
        }

        //SaveLoad
        public void WriteToBinaryFile(BinaryWriter bin)
        {
            bin.Write(roomName);
            bin.Write((byte)layers.Count);
            bin.Write((short)RoomGlobalPosX);
            bin.Write((short)RoomGlobalPosY);
            bin.Write((ushort)RoomWidth);
            bin.Write((ushort)RoomHeight);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].WriteToBinaryFile(bin);
            }
        }
        public static Room ReadFromBinaryFile(BinaryReader bin)
        {
            Console.WriteLine("stream position: {0}", bin.BaseStream.Position);
            string name = bin.ReadString();
            byte layerCount = bin.ReadByte();
            short x = bin.ReadInt16();
            short y = bin.ReadInt16();
            ushort width = bin.ReadUInt16();
            ushort height = bin.ReadUInt16();
            Room newRoom = new Room(name, x, y, width, height);
            Console.WriteLine(layerCount);

            for (int i = 0; i < layerCount; i++)
            {
                AbstractLayer a = AbstractLayer.ReadFromBinaryFile(bin, newRoom);
                newRoom.AddLayer(a);
            }

            return newRoom;
        }
        public void InitialiseLayerTextures()
        {
            foreach (AbstractLayer layer in layers)
            {
                layer.InitialiseTextures();
            }
        }

        //Constructor
        public Room(string name, int worldPosX, int worldPosY, int width, int height)
        {
            this.roomName = name;
            this.RoomGlobalPosX = worldPosX;
            this.RoomGlobalPosY = worldPosY;
            this.RoomWidth = width;
            this.RoomHeight = height;
            TexManager = new TextureManager();
        }
    }
}