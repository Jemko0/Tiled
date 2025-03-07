using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tiled.UI.Widgets
{
    public class WProgressBar : Widget
    {
        public Texture2D backgroundTexture;
        public Texture2D fillTexture;

        public Color backgroundColor = Color.Black;
        public Color fillColor = Color.Red;

        public float value;
        public float minValue;
        public float maxValue;
        public float valueNormalized;

        public WProgressBar(HUD owner) : base(owner)
        {
        }

        public override void Construct()
        {
            backgroundTexture = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
            backgroundTexture.SetData(new Color[] { Color.White });
            fillTexture = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
            fillTexture.SetData(new Color[] { Color.White });
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            Update();
            Rectangle dest = scaledGeometry;
            dest.Width = (int)(dest.Width * valueNormalized);
            dest.Inflate(-2 * HUD.DPIScale, -2 * HUD.DPIScale);

            sb.Draw(backgroundTexture, scaledGeometry, backgroundColor);
            sb.Draw(fillTexture, dest, fillColor);
        }

        public void Update()
        {
            valueNormalized = (value - minValue) / (maxValue - minValue);
        }
    }
}