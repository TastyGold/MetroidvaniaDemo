using System.Numerics;
using Raylib_cs;

namespace MapEditor
{
    public abstract partial class BaseWindow : IResizableWindow
    {
        //Window data
        public int windowWidth;
        public int windowHeight;
        public int windowScreenX;
        public int windowScreenY;
        protected RenderTexture2D renderTexture;
        protected Color windowBackgroundColor = Color.WHITE;
        protected bool isMouseOver = false;

        //Mouse Interaction
        protected Vector2 mouseCurrentPosition;
        protected Vector2 mouseDownPosition;
        protected Vector2 mouseLastPosition;
        protected Vector2 MouseDeltaPosition => mouseCurrentPosition - mouseLastPosition;

        protected void UpdateMouseState()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            mouseLastPosition = mouseCurrentPosition;
            mouseCurrentPosition = mousePos;
            isMouseOver = !(mousePos.X < windowScreenX || mousePos.X > windowScreenX + windowWidth || mousePos.Y < windowScreenY || mousePos.Y > windowScreenY + windowHeight);
        }

        protected Camera2D NewWindowCamera => new Camera2D
        {
            offset = new Vector2(windowWidth / 2, windowHeight / 2),
            target = new Vector2(windowWidth / 2, windowHeight / 2),
            zoom = 1f
        };

        //Methods
        public void BeginDrawing()
        {
            Raylib.BeginTextureMode(renderTexture);
        }
        public void ClearBackground()
        {
            Raylib.ClearBackground(windowBackgroundColor);
        }
        public void EndDrawing()
        {
            Raylib.EndTextureMode();
        }
        public void DrawToScreen()
        {
            Raylib.DrawTextureRec(renderTexture.texture, new Rectangle(0, 0, windowWidth, -windowHeight), new Vector2(windowScreenX, windowScreenY), Color.WHITE);
        }
        public void Resize(int width, int height)
        {
            windowWidth = width;
            windowHeight = height;
            renderTexture = Raylib.LoadRenderTexture(width, height);
        }
        
        //Constructors
        public BaseWindow() { }
        public BaseWindow(int x, int y, int width, int height)
        {
            windowScreenX = x;
            windowScreenY = y;
            windowWidth = width;
            windowHeight = height;
            renderTexture = Raylib.LoadRenderTexture(width, height);
        }
    }
}