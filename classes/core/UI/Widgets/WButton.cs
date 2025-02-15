using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.Input;

namespace Tiled.UI
{
    public class WButton : PanelWidget
    {
        public delegate void ButtonPressed(ButtonPressArgs args);
        public event ButtonPressed onButtonPressed;

        public WButton(HUD owner) : base(owner)
        {
        }

        public Texture2D buttonTexture;
        public Texture2D buttonHoveredTexture;
        public override void Construct()
        {
            buttonTexture = Program.GetGame().Content.Load<Texture2D>("UI/button/btn-default");
            buttonHoveredTexture = Program.GetGame().Content.Load<Texture2D>("UI/button/btn-default-hover");
            InputManager.onLeftMousePressed += OnLeftMousePressed;
            base.Construct();
        }

        private void OnLeftMousePressed(MouseButtonEventArgs e)
        {
            if(IsHovered())
            {
                if(onButtonPressed != null)
                {
                    onButtonPressed.Invoke(new ButtonPressArgs(e.position, e.mouseButton));
                }
            }
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            sb.Draw(IsHovered()? buttonHoveredTexture : buttonTexture, scaledGeometry, Color.LightSkyBlue);
            base.DrawWidget(ref sb);
        }

        public override void DrawChild(ref SpriteBatch sb, int childIdx)
        {
            children[childIdx].SetGeometry(GetSize(), AnchorPosition.TopLeft);
            children[childIdx].Draw(ref sb);
        }
    }
}
