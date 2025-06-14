﻿using Microsoft.Xna.Framework.Graphics;

namespace Tiled.UI
{
    /// <summary>
    /// works like a default widget but has the ability to render childern using <see cref="DrawChildren(ref SpriteBatch)"/> and <see cref="DrawChild(ref SpriteBatch, int)"/>
    /// </summary>
    public class PanelWidget : Widget
    {
        public int maxChildIndex = int.MaxValue;
        public PanelWidget(HUD owner) : base(owner)
        {
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            DrawChildren(ref sb);
        }

        public virtual void DrawChildren(ref SpriteBatch sb)
        {
            if(children == null)
            {
                return;
            }

            for (int i = 0; i < children.Count; i++)
            {
                if(children[i] == null )
                {
                    continue;
                }

                DrawChild(ref sb, i);
            }
        }

        public virtual void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            children[childIdx].Draw(ref sb);
        }
    }
}
