using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tiled.UI.UserWidgets
{
    public class UWEscapeMenu : PanelWidget
    {
        public UWEscapeMenu(HUD owner) : base(owner)
        {
        }
        public int vbWidth = 256;
        WVerticalBox vb;
        WButton resumeBtn;
        WText resumeBtnText;

        WButton exitWorldBtn;
        WText exitWorldBtnText;

        WButton settingsBtn;
        WText settingsBtnText;

        public override void Construct()
        {
            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.SetGeometry(new Vector2(vbWidth, 256), DataStructures.AnchorPosition.Center);
            vb.AttachToParent(this);

            resumeBtn = HUD.CreateWidget<WButton>(owningHUD);
            resumeBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            resumeBtn.layerDepth = 1.0f;
            resumeBtn.AttachToParent(vb);
            resumeBtn.onButtonPressed += ContinueButton;
            

            resumeBtnText = HUD.CreateWidget<WText>(owningHUD);
            resumeBtnText.text = "Continue";
            resumeBtnText.justification = DataStructures.ETextJustification.Center;
            resumeBtnText.AttachToParent(resumeBtn);

            settingsBtn = HUD.CreateWidget<WButton>(owningHUD);
            settingsBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            settingsBtn.layerDepth = 1.0f;
            settingsBtn.AttachToParent(vb);
            settingsBtn.onButtonPressed += OnSettingsButtonPressed;

            settingsBtnText = HUD.CreateWidget<WText>(owningHUD);
            settingsBtnText.text = "Settings";
            settingsBtnText.justification = DataStructures.ETextJustification.Center;
            settingsBtnText.AttachToParent(settingsBtn);

            exitWorldBtn = HUD.CreateWidget<WButton>(owningHUD);
            exitWorldBtn.SetGeometry(new Vector2(0, 72), DataStructures.AnchorPosition.Center);
            exitWorldBtn.layerDepth = 1.0f;
            exitWorldBtn.AttachToParent(vb);
            exitWorldBtn.onButtonPressed += ExitButtonPressed;

            exitWorldBtnText = HUD.CreateWidget<WText>(owningHUD);
            exitWorldBtnText.text = "Exit World";
            exitWorldBtnText.justification = DataStructures.ETextJustification.Center;
            exitWorldBtnText.AttachToParent(exitWorldBtn);
        }

        private void OnSettingsButtonPressed(DataStructures.ButtonPressArgs args)
        {
            var s = HUD.CreateWidget<UWSettings>(owningHUD);
            Program.GetGame().GetLocalPlayer().settingsWidget = s;
            s.SetGeometry(new Vector2(1920, 1080), DataStructures.AnchorPosition.Center);
            s.inGame = true;
            DestroyWidget();
        }

        private void ExitButtonPressed(DataStructures.ButtonPressArgs args)
        {
            for(int i = 0; i < Main.entities.Count; i++)
            {
                Main.entities[i].Destroy();
            }

            Main.entities.Clear();
            Main.inTitle = true;

            var t = HUD.CreateWidget<UWTitle>(owningHUD);
            t.SetGeometry(new Vector2(1920, 1080), DataStructures.AnchorPosition.Center);
            Main.escMenuOpen = false;

            World.renderWorld = false;
            World.tiles = null;
            World.walls = null;
            World.wallFramesCached = null;
            World.tileFramesCached = null;
            World.tileBreak = null;
            World.lightMap = null;

            GC.Collect();
            DestroyWidget();
        }

        private void ContinueButton(DataStructures.ButtonPressArgs args)
        {
            Main.escMenuOpen = false;
            DestroyWidget();
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            SetGeometry(new Vector2(vbWidth, 0), DataStructures.AnchorPosition.Center);
            base.DrawWidget(ref sb);
        }
    }
}
