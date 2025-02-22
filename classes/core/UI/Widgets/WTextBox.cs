using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using Tiled.Input;
using Tiled.UI.Font;

namespace Tiled.UI
{
    public class WTextBox : PanelWidget
    {
        public bool isFocused = false;
        bool functionBound = false;
        public string hintText = "Enter text here...";
        public string text = "";
        KeyboardState oldState;
        Texture2D backgroundTexture;
        public WTextBox(HUD owner) : base(owner)
        {

        }

        public override void Construct()
        {
            layerDepth = 0.9f;
            InputManager.onLeftMousePressed += OnLeftMousePressed;
            backgroundTexture = Program.GetGame().Content.Load<Texture2D>("UI/textbox/textbox-bg-default");
        }

        private void OnLeftMousePressed(MouseButtonEventArgs e)
        {
            isFocused = IsHovered();
            UpdateFocused();
        }

        private void UpdateFocused()
        {
            Program.GetGame().localPlayerController.ignoreInput = isFocused;

            if (isFocused)
            {
                if(!functionBound)
                {
                    Program.GetGame().Window.TextInput += WTextBox_onKeyPressed;
                    functionBound = true;
                }
            }
            else
            {
                if(functionBound)
                {
                    Program.GetGame().Window.TextInput -= WTextBox_onKeyPressed;
                    functionBound = false;
                }
            }
        }

        private void WTextBox_onKeyPressed(object sender, TextInputEventArgs e)
        {
            if(e.Key == Keys.Back && text.Length > 0)
            {
                text = text.Remove(text.Length - 1);
                return;
            }

            if (e.Key == Keys.Enter)
            {
                isFocused = false;
                UpdateFocused();
                return;
            }

            if(e.Character != '\b')
            {
                text += e.Character;
            }
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            Vector2 originOffset = new(0.0f, 0.0f);
            Vector2 originTextOffset = new(0.0f, 0.0f);
            DrawTextBoxBackground(ref sb);

            if ((text.Length > 0) && text != "\b")
            {
                sb.DrawString(Fonts.Andy_24pt, text, new Vector2(scaledGeometry.X + 5 * HUD.DPIScale, scaledGeometry.Y + 0 * HUD.DPIScale), Color.White, 0.0f, originTextOffset, HUD.DPIScale, SpriteEffects.None, 0.8f);
            }
            else
            {
                sb.DrawString(Fonts.Andy_24pt, hintText, new Vector2(scaledGeometry.X + 5 * HUD.DPIScale, scaledGeometry.Y + 0 * HUD.DPIScale), Color.Gray, 0.0f, originTextOffset, HUD.DPIScale, SpriteEffects.None, 0.8f);
            }
        }

        public void DrawTextBoxBackground(ref SpriteBatch sb)
        {
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
            sb.Draw(backgroundTexture, leftDest, leftSrc, drawColor, 0.0f, new(0.0f), SpriteEffects.None, layerDepth);
            sb.Draw(backgroundTexture, rightDest, rightSrc, drawColor, 0.0f, new(0.0f), SpriteEffects.None, layerDepth);

            sb.Draw(backgroundTexture, middleDest, middleSrc, drawColor, 0.0f, new(0.0f), SpriteEffects.None, layerDepth);
        }
    }
}
