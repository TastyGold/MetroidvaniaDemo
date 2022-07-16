using System;
using System.IO;
using System.Collections.Generic;
using Raylib_cs;
using ErrorLogging;
using MapEditor;

namespace TextureAtlases
{
    public class TextureAtlas
    {
        public const string Directory = Directories.Graphics;

        //Properties
        public string TextureId { get; private set; }
        public string PathInDirectory { get; private set; } //path to the .png file in the graphics directory
        public Texture2D Texture { get; private set; }
        public List<SpriteInfo> Sprites { get; private set; }

        //Methods
        private List<SpriteInfo> ReadSpritesFromCSV(string filePath)
        {
            List<SpriteInfo> sprites = new List<SpriteInfo>();

            if (!File.Exists(filePath))
            {
                ErrorLogger.LogFileNotFound(filePath);
                throw new FileNotFoundException();
            }

            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] elements = lines[i].Split(',');
                sprites.Add(new SpriteInfo()
                {
                    AtlasIndex = i - 1,
                    Id = elements[0],
                    X = Convert.ToInt32(elements[1]),
                    Y = Convert.ToInt32(elements[2]),
                    Width = Convert.ToInt32(elements[3]),
                    Height = Convert.ToInt32(elements[4]),
                    OffsetX = Convert.ToInt32(elements[5]),
                    OffsetY = Convert.ToInt32(elements[6]),
                });
            }

            return sprites;
        }
        public SpriteInfo GetSprite(string id)
        {
            int index = Sprites.FindIndex(m => m.Id == id);
            if (index >= 0)
            {
                return Sprites[index];
            }
            else throw new Exception($"Texture atlas does not contain sprite Id: {id}");
        }
        public Rectangle GetSpriteRec(int index)
        {
            return new Rectangle(Sprites[index].X, Sprites[index].Y, Sprites[index].Width, Sprites[index].Height);
        }
        public Rectangle GetSpriteRec(string id)
        {
            int index = Sprites.FindIndex(m => m.Id == id);
            if (index >= 0)
            {
                return GetSpriteRec(index);
            }
            else throw new Exception($"Texture atlas does not contain sprite Id: {id}");
        }

        public string GetPNGPath()
        {
            return (Directory + PathInDirectory);
        }
        public string GetCSVPath()
        {
            return GetPNGPath().Replace(".png", ".csv");
        }

        public override bool Equals(object obj)
        {
            return obj is TextureAtlas atlas &&
                   TextureId == atlas.TextureId &&
                   PathInDirectory == atlas.PathInDirectory &&
                   EqualityComparer<List<SpriteInfo>>.Default.Equals(Sprites, atlas.Sprites);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(TextureId, PathInDirectory, Texture, Sprites);
        }

        //Constructors
        public TextureAtlas(TextureManager manager, string TexId, string directory)
        {
            TextureId = TexId;
            PathInDirectory = TexId + ".png";
            Texture = manager.RequestTexture(directory + PathInDirectory);
            Sprites = ReadSpritesFromCSV(directory + TexId + ".csv");
        }
        public TextureAtlas(TextureManager manager, string pathInGraphicsDir)
        {
            TextureId = Path.GetFileNameWithoutExtension(pathInGraphicsDir);
            PathInDirectory = pathInGraphicsDir;
            Texture = manager.RequestTexture(GetPNGPath());
            Sprites = ReadSpritesFromCSV(GetCSVPath());
        }
    }
}