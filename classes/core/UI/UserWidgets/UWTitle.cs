using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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

        WButton settingsBtn;
        WText settingsBtnText;

        WButton exitBtn;
        WText exitBtnText;

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

            settingsBtn = HUD.CreateWidget<WButton>(owningHUD);
            settingsBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            settingsBtn.layerDepth = 1.0f;
            settingsBtn.AttachToParent(vb);
            settingsBtn.onButtonPressed += OnSettingsButtonPressed;

            settingsBtnText = HUD.CreateWidget<WText>(owningHUD);
            settingsBtnText.text = "Settings";
            settingsBtnText.justification = DataStructures.ETextJustification.Center;
            settingsBtnText.AttachToParent(settingsBtn);

            exitBtn = HUD.CreateWidget<WButton>(owningHUD);
            exitBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            exitBtn.onButtonPressed += ExitBtn_onButtonPressed;
            exitBtn.layerDepth = 1.0f;
            exitBtn.AttachToParent(vb);

            exitBtnText = HUD.CreateWidget<WText>(owningHUD);
            exitBtnText.text = "Exit";
            exitBtnText.justification = DataStructures.ETextJustification.Center;
            exitBtnText.AttachToParent(exitBtn);

            WScrollBox scrollbar = HUD.CreateWidget<WScrollBox>(owningHUD);
            scrollbar.SetGeometry(new Vector2(500, 1000), null);
            scrollbar.AttachToParent(this);
            scrollbar.SetOffset(new Vector2(-700, -500));

            for (int i = 0; i < 50; i++)
            {
                WButton b = HUD.CreateWidget<WButton>(owningHUD);
                b.SetGeometry(new Vector2(0, new Random().NextSingle() + 1.0f * 48.0f), DataStructures.AnchorPosition.Center);
                //b.layerDepth = 1.0f;
                b.AttachToParent(scrollbar);

                WText t = HUD.CreateWidget<WText>(owningHUD);
                t.text = "SCROLL " + i;
                t.fontScale = 0.5f;
                t.justification = DataStructures.ETextJustification.Center;
                t.AttachToParent(b);
            }
            
        }

        private void ExitBtn_onButtonPressed(DataStructures.ButtonPressArgs args)
        {
            Program.GetGame().Exit();
        }

        private void OnSettingsButtonPressed(DataStructures.ButtonPressArgs args)
        {
            var s = HUD.CreateWidget<UWSettings>(owningHUD);
            s.SetGeometry(new Vector2(1920, 1080), DataStructures.AnchorPosition.Center);
            DestroyWidget();
        }

        private void MultiplayerButtonPressed(DataStructures.ButtonPressArgs args)
        {
            var t = HUD.CreateWidget<UWMultiplayerTab>(owningHUD);
            t.SetGeometry(new Vector2(256, 0), DataStructures.AnchorPosition.Center);
            DestroyWidget();
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
