using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

using Tiled.DataStructures;

namespace Tiled.UI
{
    public class Widget : IDisposable
    {
        public bool visible = true;
        public delegate void WidgetDestroyed(WidgetDestroyArgs e);
        public event WidgetDestroyed onWidgetDestroyed;
        public HUD owningHUD;
        public Vector2 origin;
        public Rectangle geometry;
        public Rectangle scaledGeometry;
        public Widget(HUD owner)
        {
            owningHUD = owner;
        }

        public virtual void Construct()
        {

        }


        /// <summary>
        /// call this to set position, scale. Also handles ORIGIN
        /// </summary>
        /// <param name="newGeo"></param>
        public void SetGeometry(Rectangle newGeo, Vector2? newOrigin = null, bool setOrigin = false)
        {
            geometry = newGeo;

            if(setOrigin && newOrigin != null)
            {
                origin.X = newOrigin.Value.X;
                origin.Y = newOrigin.Value.Y;
            }
            
            ScaleGeometry();
        }

        public void ScaleGeometry()
        {
            scaledGeometry.X = (int)origin.X;
            scaledGeometry.Y = (int)origin.Y;

            scaledGeometry.X = (int)(scaledGeometry.X * HUD.DPIScale);
            scaledGeometry.Y = (int)(scaledGeometry.Y * HUD.DPIScale);

            scaledGeometry.X += geometry.X;
            scaledGeometry.Y += geometry.Y;

            scaledGeometry.Width = (int)(geometry.Width * HUD.DPIScale);
            scaledGeometry.Height = (int)(geometry.Height * HUD.DPIScale);
        }

        public void DestroyWidget()
        {
            onWidgetDestroyed.Invoke(new WidgetDestroyArgs(this));
        }
 
        public void Draw(ref SpriteBatch sb)
        {
            if (!visible)
            {
                return;
            }

            DrawWidget(ref sb);
        }

        public virtual void DrawWidget(ref SpriteBatch sb)
        {
            var tex = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
            tex.SetData(new Color[] {Color.Red});
            sb.Draw(tex, scaledGeometry, Color.White);
        }

        //disposing shit
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                   
                }

                disposed = true;
            }
        }
    }
}
