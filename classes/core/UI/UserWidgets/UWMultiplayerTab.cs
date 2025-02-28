using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.UI;
using Tiled.UI.UserWidgets;

namespace Tiled.UI
{
    public class UWMultiplayerTab : PanelWidget
    {
        public UWMultiplayerTab(HUD owner) : base(owner)
        {
        }

        WVerticalBox vb;
        WHorizontalBox hz;
        WButton joinBtn;
        WText joinBtnText;

        WButton backBtn;
        WText backBtnText;

        WTextBox txtbox;
        int vbWidth = 512;
        public override void Construct()
        {
            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.childrenKeepWidth = false;
            vb.SetGeometry(new Vector2(vbWidth, 256), null);
            vb.AttachToParent(this, AnchorPosition.TopCenter);

            txtbox = HUD.CreateWidget<WTextBox>(owningHUD);
            txtbox.SetGeometry(new Vector2(256, 32), null);
            txtbox.hintText = "IP-Address";
            txtbox.AttachToParent(vb);

            hz = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            hz.childrenKeepWidth = false;
            hz.SetGeometry(new Vector2(vbWidth, 128), AnchorPosition.TopCenter);
            hz.AttachToParent(vb, AnchorPosition.TopCenter);

            backBtn = HUD.CreateWidget<WButton>(owningHUD);
            backBtn.SetGeometry(new Vector2(128, 72), AnchorPosition.TopCenter);
            backBtn.layerDepth = 1.0f;
            backBtn.AttachToParent(hz);
            backBtn.onButtonPressed += BackPressed;

            backBtnText = HUD.CreateWidget<WText>(owningHUD);
            backBtnText.SetGeometry(new Vector2(24, 24), AnchorPosition.TopCenter);
            backBtnText.text = "Back";
            backBtnText.justification = ETextJustification.Center;
            backBtnText.layerDepth = 0.9f;
            backBtnText.AttachToParent(backBtn);


            joinBtn = HUD.CreateWidget<WButton>(owningHUD);
            joinBtn.SetGeometry(new Vector2(128, 72), AnchorPosition.TopCenter);
            joinBtn.layerDepth = 1.0f;
            joinBtn.AttachToParent(hz);
            joinBtn.onButtonPressed += JoinPressed;

            joinBtnText = HUD.CreateWidget<WText>(owningHUD);
            joinBtnText.SetGeometry(new Vector2(24, 24), AnchorPosition.TopCenter);
            joinBtnText.text = "Join";
            joinBtnText.justification = ETextJustification.Center;
            joinBtnText.layerDepth = 0.9f;
            joinBtnText.AttachToParent(joinBtn);
        }

        private void BackPressed(ButtonPressArgs args)
        {
            var title = HUD.CreateWidget<UWTitle>(owningHUD);
            title.SetGeometry(new Vector2(1920, 1080), AnchorPosition.Center);
            DestroyWidget();
        }

        private void JoinPressed(ButtonPressArgs args)
        {
#if !TILEDSERVER
            Program.GetGame().JoinServer(txtbox.text);
#endif
            DestroyWidget();

            var j = HUD.CreateWidget<UWJoinServer>(owningHUD);
            j.SetGeometry(new Vector2(1920, 1080), AnchorPosition.Center);
        }
    }
}