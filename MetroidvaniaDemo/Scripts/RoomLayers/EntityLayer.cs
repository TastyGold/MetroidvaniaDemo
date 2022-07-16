using System;
using System.IO;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public class EntityLayer : AbstractLayer
        {
            //SaveLoad
            public override byte GetLayerTypeId() => 2;

            public override void WriteToBinaryFile(BinaryWriter bin)
            {
                base.WriteToBinaryFile(bin);
            }
            public static AbstractLayer ReadLayerFromBinaryFile(BinaryReader bin, Room parentRoom)
            {
                throw new NotImplementedException();
            }

            public override void ResizeLayer(int deltaX, int deltaY, int deltaWidth, int deltaHeight)
            {
                throw new NotImplementedException();
            }

            public override void DrawAll()
            {
                throw new NotImplementedException();
            }

            public EntityLayer(Room parent) : base(parent)
            {
                throw new NotImplementedException();
            }
        }
    }
}