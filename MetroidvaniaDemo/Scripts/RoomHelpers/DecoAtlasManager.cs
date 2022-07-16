using System;
using System.Collections.Generic;
using Raylib_cs;
using TextureAtlases;

namespace MetroidvaniaLevels
{
    public partial class Room
    {
        public class DecoAtlasManager
        {
            private List<TextureAtlas> atlasList = new List<TextureAtlas>();

            public void AddTextureAtlas(TextureAtlas atlas)
            {
                atlasList.Add(atlas);
            }
            public bool ContainsAtlas(TextureAtlas atlas)
            {
                return atlasList.Contains(atlas);
            }

            public int GetAtlasIndex(TextureAtlas atlas)
            {
                int index = atlasList.FindIndex(m => m == atlas);
                if (index == -1) throw new Exception("Atlas manager does not contain atlas");
                return index;
            }
            public Texture2D GetAtlasTexture(int atlasIndex)
            {
                return atlasList[atlasIndex].Texture;
            }
            public TextureAtlas GetAtlasAt(int index)
            {
                return atlasList[index];
            }

            public SpriteInfo GetSpriteInfo(int atlasIndex, string spriteId)
            {
                return atlasList[atlasIndex].GetSprite(spriteId);
            }
            public Rectangle GetSpriteRec(int atlasIndex, string spriteId)
            {
                return atlasList[atlasIndex].GetSpriteRec(spriteId);
            }
        }
    }
}