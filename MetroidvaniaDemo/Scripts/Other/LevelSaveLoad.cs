using MapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MetroidvaniaLevels
{
    public static partial class FileSaveLoad
    {
        /* Level Binary Structures:
         * 
         * Level:
         *  RoomCount - UInt16
         *  StartingRoom - string
         *  Spare Metadata space:
         *   64 bits
         *  Rooms - Room[]
         *  
         * Room:
         *  Name - string
         *  LayerCount - byte
         *  X - Int16
         *  Y - Int16
         *  Width - UInt16
         *  Height - UInt16
         *  Layers - Layer[]
         *  
         * Layer:
         *  LayerType - byte
         *  Layerdata - <see below>
         *  
         * TileLayer (layerType 1):
         *  TextureId - byte
         *  TileArray - byte[,]
         *  
         * DecoLayer (layerType 2):
         *  Count - UInt16
         *  Decoration:
         *   TextureId - byte
         *   DecoId - byte
         *   PosX - Int16
         *   PosY - Int16
         *   Offset - byte
         *   
         * EntityLayer (layerType 3):
         *  Count - UInt16
         *  Entity:
         *   EntId - UInt16
         *   PosX - Int16
         *   PosY - Int16
         *   Offset - byte
         *   
         */

        public static string levelDataDir = Directories.Maps;

        public static void SaveLevelToFile(string filePath, Level levelData)
        {
            Console.WriteLine("Saving level: {0}", filePath);
            Stopwatch s = new Stopwatch();
            s.Start();
            using (BinaryWriter outputFile = new BinaryWriter(File.Open(Path.Combine(levelDataDir, filePath), FileMode.Create)))
            {
                levelData.WriteToBinaryFile(outputFile);
            }
            s.Stop();
            Console.WriteLine("Successfully saved: {0} ({1}ms)", filePath, s.ElapsedMilliseconds);
        }
        public static Level ReadLevelFromFile(string filePath)
        {
            if (!File.Exists(Path.Combine(levelDataDir, filePath)))
            {
                Console.WriteLine("Level does not exist");
                return new Level();
            }

            Level level = new Level();
            Console.WriteLine("Loading level: {0}", filePath);
            using (BinaryReader binaryFile = new BinaryReader(File.Open(Path.Combine(levelDataDir, filePath), FileMode.Open)))
            {
                level = Level.ReadFromBinaryFile(binaryFile);
            }
            Console.WriteLine("Successfully loaded: {0}", filePath);
            return level;
        }
    }
}