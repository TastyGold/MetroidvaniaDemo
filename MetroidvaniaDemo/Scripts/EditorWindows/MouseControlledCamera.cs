using System;
using System.Numerics;
using InputHelper;
using Raylib_cs;

namespace MapEditor
{
    public abstract partial class BaseWindow
    {
        public class MouseControlledCamera
        {
            private Camera2D cam;

            public float Zoom
            {
                get { return cam.zoom; }
                set { cam.zoom = value; }
            }
            public Camera2D Camera
            {
                get { return cam; }
            }
            public Vector2 Target
            {
                get { return cam.target; }
                set { cam.target = value; }
            }
            public float TargetX
            {
                get { return cam.target.X; }
                set { cam.target.X = value; }
            }
            public float TargetY
            {
                get { return cam.target.Y; }
                set { cam.target.Y = value; }
            }

            private readonly BaseWindow window;
            public Vector2 lowerBound;
            public Vector2 upperBound;
            public Vector2 resetOrigin;

            public void Update()
            {
                float scrollAmount = Raylib.GetMouseWheelMove();
                if (scrollAmount > 0 && cam.zoom < 4)
                {
                    cam.zoom *= 1.2f;
                }
                else if (scrollAmount < 0 && cam.zoom > 0.5f)
                {
                    cam.zoom /= 1.2f;
                }
                else if (Input.Held_MMB)
                {
                    cam.target -= window.MouseDeltaPosition / cam.zoom;
                    cam.target.X = Math.Clamp(cam.target.X, lowerBound.X, upperBound.Y);
                    cam.target.Y = Math.Clamp(cam.target.Y, lowerBound.X, upperBound.Y);
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_R))
                {
                    cam.target = resetOrigin;
                    cam.zoom = 1;
                }
            }

            public MouseControlledCamera(BaseWindow window, Camera2D camera, Vector2 lowerBound, Vector2 upperBound)
            {
                this.window = window;
                cam = camera;
                this.lowerBound = lowerBound;
                this.upperBound = upperBound;
            }
        }
    }
}