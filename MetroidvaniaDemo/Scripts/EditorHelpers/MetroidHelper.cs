using System;
using System.Collections.Generic;
using System.Text;
using Raylib_cs;

namespace MetroidHelper
{
    public static class Debleeder
    {
        public static Rectangle Debleed(this Rectangle r)
        {
            return new Rectangle(r.x + (r.x >= 0 ? 0.0001f : -0.0001f), r.y + (r.y >= 0 ? 0.0001f : -0.0001f), r.width - 0.0002f, r.height - 0.0002f);
        }
    }
}
