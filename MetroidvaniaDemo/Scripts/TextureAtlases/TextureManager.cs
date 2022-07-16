using System;
using System.Collections.Generic;
using Raylib_cs;
using MapEditor;
using System.IO;

namespace TextureAtlases
{
    public class TextureManager : IDisposable
    {
        public Dictionary<string, Texture2D> TextureDictionary { get; private set; }
        public Dictionary<string, TextureAtlas> AtlasDictionary { get; private set; }

        /// <summary>
        /// Gets a Texture2D data from dictionary. If the texture is not yet loaded, it will first load the texture to memory.
        /// </summary>
        /// <param name="filePath">File path of the requested texture.</param>
        /// <returns>Texture2D</returns>
        public Texture2D RequestTexture(string filePath)
        {
            if (!TextureDictionary.ContainsKey(filePath))
            {
                TextureDictionary.Add(filePath, Raylib.LoadTexture(filePath));
            }

            return TextureDictionary[filePath];
        }
        public TextureAtlas RequestAtlas(string filePath)
        {
            if (!AtlasDictionary.ContainsKey(filePath))
            {
                AtlasDictionary.Add(filePath, new TextureAtlas(this, Path.GetFileNameWithoutExtension(filePath), Directories.Graphics));
            }

            return AtlasDictionary[filePath];
        }

        public void UnloadAll()
        {
            foreach (KeyValuePair<string, Texture2D> pair in TextureDictionary)
            {
                Raylib.UnloadTexture(pair.Value);
            }
        }

        public void Dispose()
        {
            UnloadAll();
        }

        public TextureManager()
        {
            TextureDictionary = new Dictionary<string, Texture2D>();
            AtlasDictionary = new Dictionary<string, TextureAtlas>();
        }
    }
}