using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
        public WTextBox(HUD owner) : base(owner)
        {

        }

        public override void Construct()
        {
            layerDepth = 0.9f;
            InputManager.onLeftMousePressed += OnLeftMousePressed;
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
            sb.Draw(Program.GetGame().Content.Load<Texture2D>("UI/textbox/textbox-bg-default"), scaledGeometry, null, isFocused? Color.SteelBlue : Color.LightSkyBlue, 0.0f, originOffset, SpriteEffects.None, layerDepth);

            if ((text.Length > 0) && text != "\b")
            {
                sb.DrawString(Fonts.Andy_24pt, text, new Vector2(scaledGeometry.X + 15 * HUD.DPIScale, scaledGeometry.Y + 0 * HUD.DPIScale), Color.White, 0.0f, originTextOffset, HUD.DPIScale, SpriteEffects.None, 0.8f);
            }
            else
            {
                sb.DrawString(Fonts.Andy_24pt, hintText, new Vector2(scaledGeometry.X + 15 * HUD.DPIScale, scaledGeometry.Y + 0 * HUD.DPIScale), Color.Gray, 0.0f, originTextOffset, HUD.DPIScale, SpriteEffects.None, 0.8f);
            }
        }
    }
}
