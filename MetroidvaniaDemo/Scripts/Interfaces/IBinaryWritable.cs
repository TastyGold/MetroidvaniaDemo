using System.IO;

namespace MetroidvaniaLevels
{
    public interface IBinaryWritable
    {
        public void WriteToBinaryFile(BinaryWriter bin);
    }
}