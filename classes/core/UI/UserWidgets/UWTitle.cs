using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Networking;

namespace Tiled.UI.UserWidgets
{
    public class UWTitle : PanelWidget
    {
        public UWTitle(HUD owner) : base(owner)
        {
        }
        public int vbWidth = 256;
        WVerticalBox vb;
        WButton playBtn;
        WText playBtnText;

        WButton multiplayerBtn;
        WText multiplayerBtnText;

        public override void Construct()
        {

            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.SetGeometry(new Vector2(vbWidth, 256), DataStructures.AnchorPosition.Center);
            vb.AttachToParent(this);

            playBtn = HUD.CreateWidget<WButton>(owningHUD);
            playBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            playBtn.layerDepth = 1.0f;
            playBtn.AttachToParent(vb);
            playBtn.onButtonPressed += PlayButtonPressed;
            

            playBtnText = HUD.CreateWidget<WText>(owningHUD);
            playBtnText.text = "Play";
            playBtnText.justification = DataStructures.ETextJustification.Center;
            playBtnText.AttachToParent(playBtn);


            multiplayerBtn = HUD.CreateWidget<WButton>(owningHUD);
            multiplayerBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            multiplayerBtn.layerDepth = 1.0f;
            multiplayerBtn.AttachToParent(vb);
            multiplayerBtn.onButtonPressed += MultiplayerButtonPressed;

            multiplayerBtnText = HUD.CreateWidget<WText>(owningHUD);
            multiplayerBtnText.text = "Multiplayer";
            multiplayerBtnText.justification = DataStructures.ETextJustification.Center;
            multiplayerBtnText.AttachToParent(multiplayerBtn);
        }

        private void MultiplayerButtonPressed(DataStructures.ButtonPressArgs args)
        {
            
        }

        private void PlayButtonPressed(DataStructures.ButtonPressArgs args)
        {
            HUD.CreateWidget<UWWorldGenProgress>(owningHUD);
            DestroyWidget();
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            SetGeometry(new Vector2(vbWidth, 0), DataStructures.AnchorPosition.Center);
            base.DrawWidget(ref sb);
        }
    }
}
