using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled.UI
{
    public class PanelWidget : Widget
    {
        public PanelWidget(HUD owner) : base(owner)
        {
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            DrawChildren(ref sb);
        }

        public virtual void DrawChildren(ref SpriteBatch sb)
        {
            var tex = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
            tex.SetData(new Color[] { Color.White });

            sb.Draw(tex, scaledGeometry, Color.Blue);

            for (int i = 0; i < children.Count; i++)
            {
                DrawChild(ref sb, i);
            }
        }

        public virtual void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            children[childIdx].Draw(ref sb);
        }

        
    }
}
