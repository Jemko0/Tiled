using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Tiled.UI
{
    /// <summary>
    /// Lays children out horizontally from left to right and then after the <see cref="wrapEvery"/> amount, it loops back and goes down a row
    /// </summary>
    public class WWrapBox : PanelWidget
    {
        int wrapEvery = 5;
        float lowestY = 0;
        public WWrapBox(HUD owner) : base(owner)
        {
        }

        public void WrapEvery(int children)
        {
            lowestY = -1;
            wrapEvery = children;
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            base.DrawWidget(ref sb);
            //Texture2D white = Program.GetGame().Content.Load<Texture2D>("Entities/Item/baseAxe");
            //sb.Draw(white, scaledGeometry, Color.Blue);
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            if (childIdx >= maxChildIndex)
            {
                return;
            }
            
            int row = (int)Math.Floor((float)childIdx / wrapEvery);

            children[childIdx].anchorPosition = new Vector2(0, 0);
            children[childIdx].SetOffset(new Vector2(0, 0));

            if (childIdx != 0)
            {
                children[childIdx].SetOffset(new Vector2(children[childIdx - 1].GetSize().X * (childIdx % wrapEvery), children[childIdx].GetSize().Y * row));
            }

            children[childIdx].ScaleGeometry();
            children[childIdx].Draw(ref sb);

            if(children[childIdx].GetSize().Y * row > lowestY)
            {
                lowestY = children[childIdx].GetSize().Y * row;
                SetGeometry(new Vector2(GetSize().X, (int)lowestY), null);

                //Debug.WriteLine("WRAP BOX LOWEST Y:" + lowestY);
            }
        }
    }
}
