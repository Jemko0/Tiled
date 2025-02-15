using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.DataStructures;
using Tiled.UI.Font;

namespace Tiled.UI
{
    /// <summary>
    /// renders text on the screen, does not render children, does not support multi-line
    /// </summary>
    public class WText : Widget
    {
        public string text { get; set; } = "Hello, World!";
        public ETextJustification justification { get; set; }
        public WText(HUD owner) : base(owner)
        {
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            float x = 0.0f;
            switch(justification)
            {
                case ETextJustification.Left:
                    x = scaledGeometry.X;
                    break;

                case ETextJustification.Center:
                    x = (scaledGeometry.X + (scaledGeometry.Width / 2)) - ((Fonts.Andy_24pt.MeasureString(text).Length() * HUD.DPIScale) / 2.0f);
                    break;

                case ETextJustification.Right:
                    x = scaledGeometry.X + scaledGeometry.Width - Fonts.Andy_24pt.MeasureString(text).Length() * HUD.DPIScale;
                    break;
            }
            sb.DrawString(Fonts.Andy_24pt, text, new Vector2(x, scaledGeometry.Y + (scaledGeometry.Height / 2) - ((Fonts.Andy_24pt.LineSpacing / 2) * HUD.DPIScale)), Color.White, 0.0f, new Vector2(0, 0), HUD.DPIScale, SpriteEffects.None, 0U);
        }
    }
}
