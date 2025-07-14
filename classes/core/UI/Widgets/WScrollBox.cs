using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Tiled.Input;

namespace Tiled.UI
{
    public class WScrollBox : PanelWidget
    {
        public WScrollBox(HUD owner) : base(owner)
        {
        }

        public Texture2D scrollBarTexture;
        public Rectangle scrollBarRect;
        public float scrollBarHeight = 32f;
        public float scrollOffset = 0.0f;
        const int scrollBarWidth = 8;
        const float scrollBarPrefferedHeight = 48.0f;

        bool isDragged = false;
        private int innerPadding = 5;

        public override void Construct()
        {
            scrollBarTexture = Program.GetGame().Content.Load<Texture2D>("UI/button/btn-default");
            InputManager.onLeftMousePressed += OnLeftMousePressed;
            InputManager.onLeftMouseReleased += OnLeftMouseReleased;
            InputManager.onMouseWheel += OnMouseWheel;
            base.Construct();
        }

        private void OnMouseWheel(float axis)
        {
            if(!IsHovered() || !IsOverflowing())
            {
                return;
            }

            scrollOffset -= (axis / 120.0f) / 100.0f;
            scrollOffset = Math.Clamp(scrollOffset, 0.0f, 1.0f);
        }

        private void OnLeftMouseReleased(MouseButtonEventArgs e)
        {
            isDragged = false;
        }

        private void OnLeftMousePressed(MouseButtonEventArgs e)
        {
            if (IsScrollbarHovered())
            {
                isDragged = true;
            }
        }

        bool IsScrollbarHovered()
        {
            return scrollBarRect.Contains(Mouse.GetState().Position);
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            if(isDragged)
            {
                float screenDragOffset = Mouse.GetState().Y - scaledGeometry.Y;
                scrollOffset = Math.Clamp(screenDragOffset / scaledGeometry.Height, 0.0f, 1.0f);
            }

            if(IsOverflowing())
            {
                float contentH = GetTotalContentHeight();
                scrollBarRect = scaledGeometry;
                scrollBarRect.X = scaledGeometry.X + scaledGeometry.Width;
                scrollBarRect.Width = (int)(scrollBarWidth * HUD.DPIScale);

                scrollBarHeight = ((GetSize().Y / contentH) * scaledGeometry.Height);

                scrollBarRect.Height = (int)scrollBarHeight;

                scrollBarRect.Y = (int)MathHelper.LerpPrecise(scaledGeometry.Y, (scaledGeometry.Y + scaledGeometry.Height) - scrollBarHeight, scrollOffset);

                sb.Draw(scrollBarTexture, new Rectangle(scaledGeometry.X + scaledGeometry.Width, scaledGeometry.Y, (int)(scrollBarWidth * HUD.DPIScale), scaledGeometry.Height), new Color(0.2f, 0.2f, 0.2f));

                Color scrollBarColor = IsScrollbarHovered() || isDragged ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.9f, 0.9f, 0.9f);
                sb.Draw(scrollBarTexture, scrollBarRect, scrollBarColor);
            }

            SetClipping();
            base.DrawWidget(ref sb);
            ResetClipping();
        }

        public bool IsOverflowing()
        {
            return GetTotalContentHeight() > GetSize().Y;
        }

        public void SetClipping()
        {
            Rectangle clippingRegion = scaledGeometry;
            clippingRegion.Width += (int)(scrollBarWidth * HUD.DPIScale);
            Program.GetGame().GraphicsDevice.ScissorRectangle = clippingRegion;
        }

        public void ResetClipping()
        {
            Rectangle clippingRegion = Program.GetGame().Window.ClientBounds;
            clippingRegion.X = 0;
            clippingRegion.Y = 0;
            Program.GetGame().GraphicsDevice.ScissorRectangle = clippingRegion;
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            Vector2 childSz = children[childIdx].GetSize();

            childSz.X = GetSize().X;
            children[childIdx].SetGeometry(childSz, DataStructures.AnchorPosition.TopLeft);
            children[childIdx].anchorPosition = new Vector2(0, 0);
            
            if(childIdx != 0)
            {
                children[childIdx].SetOffset(new Vector2(0, (children[childIdx - 1].GetSize().Y * childIdx) - ((scrollOffset * GetTotalContentHeight()) - (GetSize().Y * scrollOffset))));
            }
            else
            {
                children[childIdx].SetOffset(new Vector2(0, 0 - ((scrollOffset * GetTotalContentHeight()) - (GetSize().Y * scrollOffset))));
            }

            children[childIdx].ScaleGeometry();
            children[childIdx].Draw(ref sb);
        }

        float GetTotalContentHeight()
        {
            float totalContentHeight = scaledGeometry.Height;

            if(GetChildren().Count == 0)
            {
                return totalContentHeight;
            }

            totalContentHeight = 0;

            GetChildren().ForEach(child =>
            {
                totalContentHeight += child.GetSize().Y;
            });

            return totalContentHeight;
        }
    }
}
