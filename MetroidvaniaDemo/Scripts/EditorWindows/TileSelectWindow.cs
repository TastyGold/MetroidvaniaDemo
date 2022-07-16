using System;
using System.Numerics;
using Raylib_cs;
using MetroidvaniaRuntime;
using InputHelper;

namespace MapEditor
{
    public class TileSelectWindow : BaseWindow, IBehavedWindow
    {
        //Runtime
        public int selectedTile = 0;
        private MouseControlledCamera tileCamera;
        private Texture2D tilesheet = Raylib.LoadTexture("..//..//..//maintilesheet.png");

        private int TextureScale => Screen.pixelScale;

        //Methods
        private void DrawTilePalette()
        {
            Raylib.DrawTextureEx(tilesheet, Vector2.Zero, 0, TextureScale, Color.WHITE);
        }
         private void DrawPaletteGrid()
        {
            Color gridColor = new Color(200, 200, 200, 60);

            for (int x = 0; x <= 16; x++)
            {
                Raylib.DrawLine(x * 16 * TextureScale, 0, x * 16 * TextureScale, 256 * TextureScale, gridColor);
            }
            for (int y = 0; y <= 16; y++)
            {
                Raylib.DrawLine(0, y * 16 * TextureScale, 256 * TextureScale, y * 16 * TextureScale, gridColor);
            }
        }
        private void DrawSelectedHighlight()
        {
            Raylib.DrawRectangleLinesEx(new Rectangle(
                (selectedTile % 16) * (16 * TextureScale),
                (selectedTile / 16) * (16 * TextureScale),
                16 * TextureScale, 16 * TextureScale),
                (int)(2 / tileCamera.Zoom), Color.SKYBLUE);
        }
        public void RunWindowBehaviour()
        {
            HandleMouseControl();

            BeginDrawing();
            ClearBackground();
            Raylib.BeginMode2D(tileCamera.Camera);

            DrawTilePalette();
            if (isMouseOver) DrawPaletteGrid();
            DrawSelectedHighlight();

            Raylib.EndMode2D();
            EndDrawing();
            DrawToScreen();
        }

        //Mouse interaction
        private void HandleMouseControl()
        {
            UpdateMouseState();
            if (isMouseOver)
            {
                tileCamera.Update();
                if (Input.Clicked_LMB)
                {
                    Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(mouseCurrentPosition - new Vector2(windowScreenX, windowScreenY), tileCamera.Camera);

                    int mouseTileX = (int)(mouseWorldPos.X / (16 * TextureScale));
                    int mouseTileY = (int)(mouseWorldPos.Y / (16 * TextureScale));

                    selectedTile = (mouseTileX % 16) + (mouseTileY * 16);
                }
            }
        }

        //Constructor
        public TileSelectWindow()
        {
            int rWidth = EditorManager.rightMenuWidth;
            windowScreenX = EditorManager.screenWidth - rWidth;
            windowScreenY = EditorManager.rightHeaderHeight;
            windowWidth = rWidth;
            windowHeight = rWidth;
            windowBackgroundColor = new Color(70, 70, 70, 255);
            renderTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
            tileCamera = new MouseControlledCamera(this, NewWindowCamera, Vector2.Zero, new Vector2(256 * Screen.pixelScale))
            {
                resetOrigin = new Vector2(128 * Screen.pixelScale)
            };
        }
    }
}