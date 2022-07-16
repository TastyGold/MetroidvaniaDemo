using System;
using TextureAtlases;
using System.Numerics;
using System.Collections.Generic;
using MetroidvaniaRuntime;
using Raylib_cs;
using InputHelper;

namespace MapEditor
{
    public class DecoSelectWindow : BaseWindow, IBehavedWindow
    {
        //Data
        private readonly TextureManager textures;
        private readonly TextureAtlas atlas;
        private readonly MouseControlledCamera camera;

        public int selectedSprite = -1;
        private int mouseOverSprite = -1;

        public void RunWindowBehaviour()
        {
            HandleMouseControl();

            BeginDrawing();
            ClearBackground();
            Raylib.BeginMode2D(camera.Camera);

            DrawAtlasBG();
            if (selectedSprite != -1) DrawSelectedBG();
            DrawAtlas();
            DrawSpriteOutlines();
            if (selectedSprite != -1) DrawOutline(atlas.Sprites[selectedSprite], blue: true);
            if (selectedSprite != mouseOverSprite && mouseOverSprite != -1) DrawOutline(atlas.Sprites[mouseOverSprite], blue: true);

            Raylib.EndMode2D();

            DrawSpriteInfoText();

            EndDrawing();
            DrawToScreen();
        }

        //Draw methods
        private void DrawAtlasBG()
        {
            Raylib.DrawRectangle(0, 0, atlas.Texture.width * Screen.pixelScale, atlas.Texture.height * Screen.pixelScale, new Color(75, 75, 75, 255));
            DrawOutline(new SpriteInfo() { AtlasIndex = -1, X = 0, Y = 0, Width = atlas.Texture.width, Height = atlas.Texture.height });
        }
        private void DrawSelectedBG()
        {
            SpriteInfo s = atlas.Sprites[selectedSprite];
            int ps = Screen.pixelScale;
            Raylib.DrawRectangle(s.X * ps, s.Y * ps, s.Width * ps, s.Height * ps, new Color(72, 94, 102, 255));
        }
        private void DrawAtlas()
        {
            Raylib.DrawTextureEx(atlas.Texture, Vector2.Zero, 0, Screen.pixelScale, Color.WHITE);
        }
        private void DrawSpriteOutlines()
        {
            atlas.Sprites.ForEach(s => DrawOutline(s));
        }
        private void DrawOutline(SpriteInfo s, bool blue = false)
        {
            //Color outlineColor = s.AtlasIndex == selectedSprite ? new Color(150, 150, 150, 255) : new Color(120, 120, 120, 255);
            Color outlineColor = blue ? new Color(117, 188, 255, 255) : new Color(120, 120, 120, 255);
            Raylib.DrawLineV(new Vector2(s.X, s.Y) * Screen.pixelScale, new Vector2(s.X + s.Width, s.Y) * Screen.pixelScale, outlineColor);
            Raylib.DrawLineV(new Vector2(s.X, s.Y) * Screen.pixelScale, new Vector2(s.X, s.Y + s.Height) * Screen.pixelScale, outlineColor);
            Raylib.DrawLineV(new Vector2(s.X, s.Y + s.Height) * Screen.pixelScale, new Vector2(s.X + s.Width, s.Y + s.Height) * Screen.pixelScale, outlineColor);
            Raylib.DrawLineV(new Vector2(s.X + s.Width, s.Y) * Screen.pixelScale, new Vector2(s.X + s.Width, s.Y + s.Height) * Screen.pixelScale, outlineColor);
        }
        private void DrawSelectedOutline()
        {
            SpriteInfo s = atlas.Sprites[selectedSprite];
            int ps = Screen.pixelScale;
            Raylib.DrawRectangleLinesEx(new Rectangle(s.X * ps, s.Y * ps, s.Width * ps, s.Height * ps), (int)(Screen.pixelScale / camera.Zoom), new Color(144, 188, 204, 255));
        }
        private void DrawSpriteInfoText()
        {
            string text = string.Empty;
            if (selectedSprite != -1)
            {
                text = GetSpriteInfoText(atlas.Sprites[selectedSprite]);
            }
            else
            {
                text = $"{atlas.TextureId}: {atlas.Texture.width}x{atlas.Texture.height}";
            }
            if (text != string.Empty)
            {
                Raylib.DrawTextEx(FontManager.editorFont, text, new Vector2(3 * Screen.pixelScale, windowHeight - FontManager.FontSize - (3 * Screen.pixelScale)), FontManager.FontSize, 0, new Color(185, 185, 185, 255));
            }
        }

        //Mouse methods
        private void HandleMouseControl()
        {
            UpdateMouseState();
            if (isMouseOver)
            {
                camera.Update();
                FindMouseOverSprite();
                if (Input.Clicked_LMB)
                {
                    selectedSprite = mouseOverSprite;
                }
            }
            else
            {
                mouseOverSprite = -1;
            }
        }
        private void FindMouseOverSprite()
        {
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mouseCurrentPosition - new Vector2(windowScreenX, windowScreenY), camera.Camera);
            mouseWorldPos /= Screen.pixelScale;

            bool found = false;
            for (int i = 0; i < atlas.Sprites.Count; i++)
            {
                SpriteInfo s = atlas.Sprites[i];

                if (s.X <= mouseWorldPos.X && s.Y <= mouseWorldPos.Y)
                {
                    if (s.X + s.Width >= mouseWorldPos.X && s.Y + s.Height >= mouseWorldPos.Y)
                    {
                        mouseOverSprite = i;
                        found = true;
                        i = atlas.Sprites.Count;
                    }
                }
            }

            if (found == false)
            {
                mouseOverSprite = -1;
            }
        }

        //Other methods
        private string GetSpriteInfoText(SpriteInfo s)
        {
            return $"{s.Id}: ({s.X}, {s.Y}) [{s.Width}x{s.Height}] {{{s.OffsetX}, {s.OffsetY}}}";
        }
        public TextureAtlas GetCurrentAtlas() => atlas;

        //Constructor
        public DecoSelectWindow()
        {
            int rWidth = EditorManager.rightMenuWidth;
            windowScreenX = EditorManager.screenWidth - rWidth;
            windowScreenY = EditorManager.rightHeaderHeight;
            windowWidth = rWidth;
            windowHeight = rWidth;
            windowBackgroundColor = new Color(70, 70, 70, 255);
            renderTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
            camera = new MouseControlledCamera(this, NewWindowCamera, Vector2.Zero, new Vector2(256 * Screen.pixelScale));
            camera.resetOrigin = new Vector2(128 * Screen.pixelScale);
            textures = new TextureManager();
            atlas = new TextureAtlas(textures, "atlas", Directories.Graphics);
        }
    }
}