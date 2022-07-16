using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using TextureAtlases;
using Raylib_cs;
using MetroidvaniaRuntime;
using MathExtras;
using UndoRedo;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public class DecoLayer : AbstractLayer
        {
            private readonly List<Decoration> decorations = new List<Decoration>();
            private readonly DecoAtlasManager atlasManager = new DecoAtlasManager();

            //Methods
            public override void ResizeLayer(int deltaX, int deltaY, int deltaWidth, int deltaHeight)
            {
                foreach (Decoration d in decorations)
                {
                    d.X -= deltaX * 16;
                    d.Y -= deltaY * 16;
                }
            }
            public void AddDecoration(TextureAtlas atlas, string spriteId, int x, int y, out Decoration deco, int z = 0)
            {
                if (!atlasManager.ContainsAtlas(atlas))
                {
                    atlasManager.AddTextureAtlas(atlas);
                }
                int atlasIndex = atlasManager.GetAtlasIndex(atlas);
                SpriteInfo sInfo = atlasManager.GetSpriteInfo(atlasIndex, spriteId);

                deco = new Decoration(x, y, (byte)atlasIndex, spriteId, z)
                {
                    width = sInfo.Width,
                    height = sInfo.Height
                };
                decorations.Add(deco);
            }
            public void AddDecoration(Decoration decoration) => decorations.Add(decoration);
            public void RemoveDecoration(Decoration decoration) => decorations.Remove(decoration);
            public List<Decoration> GetDecorationList() => decorations;

            //SaveLoad
            public override byte GetLayerTypeId() => 1;
            public override void WriteToBinaryFile(BinaryWriter bin)
            {
                base.WriteToBinaryFile(bin);
                Dictionary<string, List<Decoration>> atlasDecoLists = PopulateDecoDictionary();

                bin.Write((byte)atlasDecoLists.Count);
                foreach (KeyValuePair<string, List<Decoration>> pair in atlasDecoLists)
                {
                    bin.Write(pair.Key);
                    bin.Write(CountUniqueSpriteIds(pair.Value, out List<int> counts));
                    for (int i = 0, deco = 0; i < counts.Count; i++)
                    {
                        bin.Write(pair.Value[deco].SpriteID);
                        Console.WriteLine($"Freq = {counts[i] + 1}");
                        bin.Write(counts[i]+1);
                        deco += counts[i]+1;
                    }

                    foreach (Decoration d in pair.Value)
                    {
                        bin.Write((short)d.X);
                        bin.Write((short)d.Y);
                        bin.Write((short)d.Z);
                    }
                }
            }
            public static AbstractLayer ReadLayerFromBinaryFile(BinaryReader bin, Room parentRoom)
            {
                DecoLayer decoLayer = new DecoLayer(parentRoom);

                int atlasCount = bin.ReadByte();
                for (int i = 0; i < atlasCount; i++)
                {
                    string atlasPathInDir = bin.ReadString();
                    decoLayer.atlasManager.AddTextureAtlas(new TextureAtlas(parentRoom.TexManager, atlasPathInDir));
                    int numberOfUniqueIds = bin.ReadInt32();
                    Console.WriteLine($"Unique Ids = {numberOfUniqueIds}");

                    string[] ids = new string[numberOfUniqueIds];
                    int[] freqs = new int[numberOfUniqueIds];

                    for (int uid = 0; uid < numberOfUniqueIds; uid++)
                    {
                        ids[uid] = bin.ReadString();
                        freqs[uid] = bin.ReadInt32();
                        Console.WriteLine($"Freq = {freqs[uid]}");
                    }

                    for (int j = 0; j < numberOfUniqueIds; j++)
                    {
                        for (int di = 0; di < freqs[j]; di++)
                        {
                            Console.WriteLine($"Id = {ids[j]}");
                            short x = bin.ReadInt16();
                            short y = bin.ReadInt16();
                            short z = bin.ReadInt16();
                            SpriteInfo s = decoLayer.atlasManager.GetSpriteInfo(i, ids[j]);
                            Decoration d = new Decoration(x, y, (byte)i, ids[j]) { Z = z, width = s.Width, height = s.Height };
                            decoLayer.decorations.Add(d);
                        }
                    }
                }

                return decoLayer;
            }

            private Dictionary<string, List<Decoration>> PopulateDecoDictionary()
            {
                Dictionary<string, List<Decoration>> atlasDecoLists = new Dictionary<string, List<Decoration>>();
                List<Decoration> decoAlphabetically = decorations.OrderBy(o => o.SpriteID).ToList();
                for (int i = 0; i < decoAlphabetically.Count; i++)
                {
                    string atlasName = atlasManager.GetAtlasAt(decoAlphabetically[i].AtlasIndex).PathInDirectory;
                    if (!atlasDecoLists.ContainsKey(atlasName))
                        atlasDecoLists.Add(atlasName, new List<Decoration>());

                    atlasDecoLists[atlasName].Add(decoAlphabetically[i]);
                }

                return atlasDecoLists;
            }
            private int CountUniqueSpriteIds(List<Decoration> decoList, out List<int> counts)
            {
                counts = new List<int>() { 0 };
                if (decoList.Count == 0) return 0;

                int counterIndex = 0;

                for (int i = 0; i < decoList.Count - 1; i++)
                {
                    if (decoList[i].SpriteID != decoList[i + 1].SpriteID)
                    {
                        counterIndex++;
                        counts.Add(0);
                    }
                    else counts[counterIndex]++;
                }

                Console.WriteLine($"Unique spriteIds = {counterIndex + 1}");
                return counterIndex + 1;
            }

            //UndoRedo
            /// <summary>
            /// Action to add or remove a decoration.
            /// </summary>
            public class AoRDAction : IAction
            {
                private readonly Decoration decoration;
                private readonly DecoLayer layer;
                private readonly bool add;

                public void Execute()
                {
                    if (add) layer.AddDecoration(decoration);
                    else layer.RemoveDecoration(decoration);
                }
                public void Unexecute()
                {
                    if (!add) layer.AddDecoration(decoration);
                    else layer.RemoveDecoration(decoration);
                }

                /// <summary>
                /// Action for adding or removing a decoration element from a layer.
                /// </summary>
                /// <param name="layer"></param>
                /// <param name="decoration"></param>
                /// <param name="add">True for adding a decoration, false for removing.</param>
                public AoRDAction(DecoLayer layer, Decoration decoration, bool add)
                {
                    this.decoration = decoration;
                    this.layer = layer;
                    this.add = add;
                }
            }

            //Rendering
            public override void DrawAll()
            {
                foreach (Decoration d in decorations)
                {
                    DrawDecoration(d);
                }
            }
            public void DrawDecoration(Decoration d)
            {
                SpriteInfo spriteInfo = atlasManager.GetSpriteInfo(d.AtlasIndex, d.SpriteID);
                Rectangle destRec = new Rectangle(d.X + (parentRoom.RoomGlobalPosX * 16), d.Y + (parentRoom.RoomGlobalPosY * 16), spriteInfo.Width, spriteInfo.Height).Multiply(Screen.pixelScale);
                Raylib.DrawTexturePro(atlasManager.GetAtlasTexture(d.AtlasIndex), atlasManager.GetSpriteRec(d.AtlasIndex, d.SpriteID), destRec, Vector2.Zero, 0, Color.WHITE);
            }

            public DecoLayer(Room parent) : base(parent)
            {
                //nothing unique
            }
        }
    }

    public static class DecoExtensions
    {
        public static Vector2 Midpoint(this Room.Decoration d)
        {
            return new Vector2(d.X, d.Y);
        }
    }
}