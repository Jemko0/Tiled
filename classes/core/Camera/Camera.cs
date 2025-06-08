using Microsoft.Xna.Framework;
using System;

namespace Tiled
{
    public class Camera : GameComponent
    {
        public Vector2 position;
        public float zoom = 1.0f;
        public const float viewPadding = 1.0f;

        public Camera(Game game) : base(game)
        {
            position = new Vector2(0, 0);
        }
        
        public bool IsInView(System.Drawing.RectangleF rect)
        {
            float width = (Program.GetGame().Window.ClientBounds.Width / Main.renderScale) * viewPadding;
            float height = (Program.GetGame().Window.ClientBounds.Height / Main.renderScale) * viewPadding;
            
            // Calculate the visible area in world coordinates with padding
            System.Drawing.RectangleF viewRect = new System.Drawing.RectangleF(
                position.X - (Main.screenCenter.X / Main.renderScale) - (width - width / viewPadding) / 2, 
                position.Y - (Main.screenCenter.Y / Main.renderScale) - (height - height / viewPadding) / 2, 
                width,
                height);

            return viewRect.IntersectsWith(rect);
        }
    }
}
