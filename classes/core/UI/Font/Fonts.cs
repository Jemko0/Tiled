using Microsoft.Xna.Framework.Graphics;

namespace Tiled.UI.Font
{
    public static class Fonts
    {
        public static SpriteFont Andy_24pt;
        public static void InitFonts()
        {
            Andy_24pt = Program.GetGame().Content.Load<SpriteFont>("Fonts/Andy");
        }
    }
}
