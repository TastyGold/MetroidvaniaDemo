using MetroidvaniaRuntime;
using Raylib_cs;

namespace MapEditor
{
    public static class FontManager
    {
        public static int FontSize = 11 * Screen.pixelScale;
        public static Font editorFont = Raylib.LoadFontEx(Directories.Fonts + "//calibri.ttf", FontSize, null, 256);

        public static void ReloadFont(int newSize)
        {
            if (newSize > 0)
            {
                FontSize = newSize;
                Raylib.UnloadFont(editorFont);
                editorFont = Raylib.LoadFontEx(Directories.Fonts + "//calibri.ttf", FontSize, null, 256);
                OnFontReload.Invoke(newSize);
            }
        }

        public delegate void FontReloadHandler(int newFontSize);
        public static FontReloadHandler OnFontReload;
    }
}