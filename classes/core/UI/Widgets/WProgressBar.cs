using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled.UI.Widgets
{
    public class WProgressBar : Widget
    {
        public Texture2D backgroundTexture;
        public Texture2D fillTexture;

        public Color backgroundColor;
        public Color fillColor;

        public WProgressBar(HUD owner) : base(owner)
        {
        }

        public override void Construct()
        {
            backgroundTexture = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
            fillTexture = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            sb.Draw(backgroundTexture, scaledGeometry, backgroundColor);
            sb.Draw(fillTexture, scaledGeometry, fillColor);
        }
    }
}
