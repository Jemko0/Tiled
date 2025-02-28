using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
