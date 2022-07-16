using System;
using System.IO;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public abstract class AbstractLayer : IBinaryWritable
        {
            protected readonly Room parentRoom;
            public int SizeX => parentRoom.RoomWidth;
            public int SizeY => parentRoom.RoomHeight;

            //Abstract Methods
            public abstract byte GetLayerTypeId();
            public abstract void ResizeLayer(int deltaX, int deltaY, int deltaWidth, int deltaHeight);
            public abstract void DrawAll();
            public virtual void InitialiseTextures()
            {
                //do nothing
            }

            //Static Methods
            public static Type GetLayerType(int id) => id switch
            {
                0 => typeof(TilingLayer),
                1 => typeof(DecoLayer),
                2 => typeof(EntityLayer),
                _ => null,
            };
            public static AbstractLayer ReadFromBinaryFile(BinaryReader bin, Room parentRoom)
            {
                byte typeId = bin.ReadByte();
                Console.WriteLine(parentRoom.roomName);
                return typeId switch //bin.ReadByte() reads the layerType Id from stream
                {
                    0 => TilingLayer.ReadLayerFromBinaryFile(bin, parentRoom),
                    1 => DecoLayer.ReadLayerFromBinaryFile(bin, parentRoom),
                    2 => EntityLayer.ReadLayerFromBinaryFile(bin, parentRoom),
                    _ => throw new Exception($"Invalid typeId: {typeId}"),
                };
            }

            public virtual void WriteToBinaryFile(BinaryWriter bin)
            {
                bin.Write(GetLayerTypeId());
            }

            //Constructor
            public AbstractLayer(Room parent)
            {
                this.parentRoom = parent;
                parent.OnRoomTransform += ResizeLayer;
            }
        }

        public enum LayerType
        {
            Tiling,
            Deco,
            Entity,
        }
    }
}