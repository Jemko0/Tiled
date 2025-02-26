using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tiled.UI
{
    /// <summary>
    /// Lays children out horizontally from left to right
    /// </summary>
    public class WWrapBox : PanelWidget
    {
        int wrapEvery = 5;
        public WWrapBox(HUD owner) : base(owner)
        {
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            if (childIdx >= maxChildIndex)
                return;

            int row = (int)Math.Floor((float)childIdx / wrapEvery);

            children[childIdx].anchorPosition = new Vector2(0, 0);
            children[childIdx].SetOffset(new Vector2(0, 0));

            if (childIdx != 0)
            {
                children[childIdx].SetOffset(new Vector2(children[childIdx - 1].GetSize().X * (childIdx % wrapEvery), children[childIdx].GetSize().Y * row));
            }

            children[childIdx].ScaleGeometry();
            children[childIdx].Draw(ref sb);  // Keep direct Draw call
        }
    }
}
