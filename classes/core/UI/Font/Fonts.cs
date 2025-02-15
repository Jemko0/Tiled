using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
