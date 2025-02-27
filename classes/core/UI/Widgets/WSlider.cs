using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using Tiled.Input;

namespace Tiled.UI.Widgets
{
    public class WSlider : PanelWidget
    {
        public Texture2D backgroundTexture;
        public Texture2D sliderThumbTexture;
        bool isFocused;
        public float sliderValue;
        public float normalizedValue;
        public int thumbSize = 32;

        public float minValue = 0;
        public float maxValue = 1;

        public int maxDecimalPlaces = 5;

        public WSlider(HUD owner) : base(owner)
        {
        }

        public override void Construct()
        {
            backgroundTexture = Program.GetGame().Content.Load<Texture2D>("UI/Slider/slider-bg-default");
            sliderThumbTexture = Program.GetGame().Content.Load<Texture2D>("Entities/Projectile/BaseProjectile");
            InputManager.onLeftMousePressed += OnLeftMousePressed;
            InputManager.onLeftMouseReleased += OnLeftMouseReleased;
        }

        private void OnLeftMouseReleased(MouseButtonEventArgs e)
        {
            isFocused = false;
        }

        private void OnLeftMousePressed(MouseButtonEventArgs e)
        {
            isFocused = IsHovered();
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            if(isFocused)
            {
                float pixelValue = Mouse.GetState().X - scaledGeometry.X;
                normalizedValue = Math.Clamp(pixelValue / scaledGeometry.Width, 0.0f, 1.0f);
                sliderValue = (float)Math.Round(MathHelper.Lerp(minValue, maxValue, normalizedValue), maxDecimalPlaces);
                
                Debug.WriteLine(sliderValue);
            }


            Rectangle leftSrc = new Rectangle(0, 0, 6, 64);
            Rectangle rightSrc = new Rectangle(57, 0, 7, 64);

            Rectangle leftDest = scaledGeometry;
            leftDest.Width = leftSrc.Width;

            Rectangle rightDest = scaledGeometry;
            rightDest.Width = rightSrc.Width;

            rightDest.X = scaledGeometry.X + scaledGeometry.Width - rightSrc.Width;

            Rectangle middleDest = scaledGeometry;
            middleDest.X += leftSrc.Width;
            middleDest.Width -= leftSrc.Width + rightSrc.Width;

            Rectangle middleSrc = new Rectangle(8, 0, 46, 64);

            Color drawColor = isFocused ? Color.SteelBlue : Color.LightSkyBlue;
            sb.Draw(backgroundTexture, leftDest, leftSrc, drawColor, 0.0f, new Vector2(0.0f), SpriteEffects.None, layerDepth);
            sb.Draw(backgroundTexture, rightDest, rightSrc, drawColor, 0.0f, new Vector2(0.0f), SpriteEffects.None, layerDepth);

            sb.Draw(backgroundTexture, middleDest, middleSrc, drawColor, 0.0f, new Vector2(0.0f), SpriteEffects.None, layerDepth);

            sb.Draw(sliderThumbTexture, new Rectangle((int)MathHelper.Lerp(scaledGeometry.X, scaledGeometry.X + (scaledGeometry.Width - ((thumbSize / 2) * HUD.DPIScale)), normalizedValue), scaledGeometry.Center.Y - (int)((thumbSize / 2) * HUD.DPIScale), (int)(thumbSize * HUD.DPIScale), (int)(thumbSize * HUD.DPIScale)), Color.White);
        }
    }
}