using System;
using System.Collections.Generic;
using System.IO;
using TextureAtlases;

namespace MetroidvaniaLevels
{
    public class Level : IBinaryWritable, IDisposable
    {
        //Level Data
        public string startingRoom = "a_01";

        //Texture Handling
        public TextureManager TexManager { get; private set; }

        //Rooms
        public Dictionary<string, Room> RoomDictionary { get; private set; }
        public Room GetRoom(string roomName)
        {
            return RoomDictionary[roomName];
        }
        public void AddRoom(Room newRoom)
        {
            string roomName = newRoom.roomName;

            if (RoomDictionary.ContainsKey(roomName))
            {
                //Room name already taken
                Console.WriteLine($"ERROR: Room \"{roomName}\" already exists.");
                return;
            }
            else
            {
                newRoom.TexManager = TexManager;
                newRoom.InitialiseLayerTextures();
                RoomDictionary.Add(roomName, newRoom);
            }
        }
        public void AddRoom(string roomName, int positionX, int positionY, int width, int height)
        {
            AddRoom(new Room(roomName, positionX, positionY, width, height));
        }
        public void RenameRoom(string oldRoomName, string newRoomName)
        {
            if (RoomDictionary.ContainsKey(oldRoomName))
            {
                Room roomRef = RoomDictionary[oldRoomName];
                RoomDictionary.Remove(oldRoomName);

                roomRef.roomName = newRoomName;
                AddRoom(roomRef);
            }
        }

        //SaveLoad
        public void WriteToBinaryFile(BinaryWriter bin)
        {
            bin.Write((ushort)RoomDictionary.Count);
            bin.Write(startingRoom);
            bin.Write(new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 }); //currently unused level info space

            foreach (KeyValuePair<string, Room> pair in RoomDictionary)
            {
                pair.Value.WriteToBinaryFile(bin);
            }
        }
        public static Level ReadFromBinaryFile(BinaryReader bin)
        {
            Level newLevel = new Level();

            int roomCount = bin.ReadUInt16();
            Console.WriteLine("Room count: {0}", roomCount);
            newLevel.startingRoom = bin.ReadString();
            bin.ReadBytes(8); //currently unused level info space

            for (int i = 0; i < roomCount; i++)
            {
                Room newRoom = Room.ReadFromBinaryFile(bin);
                newLevel.AddRoom(newRoom);
            }

            return newLevel;
        }

        //Level Events
        public event EventHandler LevelLoaded;
        protected virtual void OnLevelLoaded(EventArgs e)
        {
            EventHandler handler = LevelLoaded;
            handler?.Invoke(this, e);
        }

        public event EventHandler LevelComplete;
        protected virtual void OnLevelComplete(EventArgs e)
        {
            EventHandler handler = LevelComplete;
            handler?.Invoke(this, e);
        }

        public void Dispose()
        {
            TexManager.UnloadAll();
        }

        public Level()
        {
            RoomDictionary = new Dictionary<string, Room>();
            TexManager = new TextureManager();
        }
    }
}