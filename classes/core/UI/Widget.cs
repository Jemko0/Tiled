using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Tiled.DataStructures;

namespace Tiled.UI
{

    /// <summary>
    /// basic widget, supports children but does not render them. if you want to render children, use <see cref="PanelWidget"/>
    /// </summary>
    public class Widget : IDisposable
    {
        public bool visible = true;
        public delegate void WidgetDestroyed(WidgetDestroyArgs e);
        public event WidgetDestroyed onWidgetDestroyed;
        public HUD owningHUD;

        protected Vector2 size;
        protected Rectangle scaledGeometry;
        public Vector2 anchorPosition; // Stores position as percentage (0-1) of screen
        protected AnchorPosition anchor;
        protected Vector2 offset; // Optional pixel offset from anchor point

        protected Widget parent;
        protected List<Widget> children = new List<Widget>();

        public Widget(HUD owner)
        {
            owningHUD = owner;
        }

        public virtual void Construct()
        {

        }

        protected Rectangle GetParentBounds()
        {
            if (parent != null)
                return parent.scaledGeometry;

            return new Rectangle(0, 0,
                (int)(Program.GetGame().Window.ClientBounds.Width),
                (int)(Program.GetGame().Window.ClientBounds.Height));
        }

        public void SetOffset(Vector2 newOffset)
        {
            offset = newOffset;
            //ScaleGeometry();
        }

        public Vector2 GetSize()
        {
            return size;
        }

        public void AttachToParent(Widget parentWidget, AnchorPosition anchorPos, Vector2? pixelOffset = null)
        {
            if (parent != null)
            {
                parent.children.Remove(this);
            }

            parent = parentWidget;
            if (parent != null)
            {
                parent.children.Add(this);
            }

            anchor = anchorPos;
            offset = pixelOffset ?? Vector2.Zero;
            CalculateRelativePosition();
            ScaleGeometry();
        }

        public void DetachFromParent()
        {
            if (parent != null)
            {
                parent.children.Remove(this);
                parent = null;

                CalculateRelativePosition();
                ScaleGeometry();
            }
        }

        private void CalculateRelativePosition()
        {
            Rectangle bounds = GetParentBounds();

            switch (anchor)
            {
                case AnchorPosition.TopLeft:
                    anchorPosition = new Vector2(0, 0);
                    break;
                case AnchorPosition.TopCenter:
                    anchorPosition = new Vector2(0.5f, 0);
                    break;
                case AnchorPosition.TopRight:
                    anchorPosition = new Vector2(1, 0);
                    break;
                case AnchorPosition.MiddleLeft:
                    anchorPosition = new Vector2(0, 0.5f);
                    break;
                case AnchorPosition.Center:
                    anchorPosition = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPosition.MiddleRight:
                    anchorPosition = new Vector2(1, 0.5f);
                    break;
                case AnchorPosition.BottomLeft:
                    anchorPosition = new Vector2(0, 1);
                    break;
                case AnchorPosition.BottomCenter:
                    anchorPosition = new Vector2(0.5f, 1);
                    break;
                case AnchorPosition.BottomRight:
                    anchorPosition = new Vector2(1, 1);
                    break;
            }
        }

        public void ScaleGeometry()
        {
            Rectangle bounds = GetParentBounds();

            // Calculate base position from relative coordinates
            Vector2 basePosition = new Vector2(
                bounds.X + (bounds.Width * anchorPosition.X),
                bounds.Y + (bounds.Height * anchorPosition.Y)
            );

            // Apply DPI scaling to the geometry dimensions
            float scaledWidth = size.X * HUD.DPIScale;
            float scaledHeight = size.Y * HUD.DPIScale;

            // Adjust position based on anchor point and widget size
            float finalX = basePosition.X;
            float finalY = basePosition.Y;

            // Adjust X position based on horizontal anchor
            if (anchorPosition.X == 0.5f)
                finalX -= scaledWidth / 2;
            else if (anchorPosition.X == 1)
                finalX -= scaledWidth;

            // Adjust Y position based on vertical anchor
            if (anchorPosition.Y == 0.5f)
                finalY -= scaledHeight / 2;
            else if (anchorPosition.Y == 1)
                finalY -= scaledHeight;

            // Apply offset
            finalX += offset.X * HUD.DPIScale;
            finalY += offset.Y * HUD.DPIScale;

            // Set final scaled geometry
            scaledGeometry = new Rectangle(
                (int)finalX,
                (int)finalY,
                (int)scaledWidth,
                (int)scaledHeight
            );

            // Update all children
            foreach (var child in children)
            {
                child.ScaleGeometry();
            }
        }

        public void SetGeometry(Vector2 newSize, AnchorPosition anchorPos, Vector2? pixelOffset = null)
        {
            size = newSize;
            anchor = anchorPos;
            offset = pixelOffset ?? Vector2.Zero;

            CalculateRelativePosition();
            ScaleGeometry();
        }

        public bool IsHovered()
        {
            return scaledGeometry.Contains(Mouse.GetState().X, Mouse.GetState().Y);
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
