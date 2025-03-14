﻿using Microsoft.Xna.Framework;
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
        int vbWidth = 256;
        public override void Construct()
        {
            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.childrenKeepWidth = true;
            vb.SetGeometry(new Vector2(vbWidth, 256), AnchorPosition.TopCenter);
            vb.AttachToParent(this, AnchorPosition.TopCenter);


            hz = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            hz.SetGeometry(new Vector2(vbWidth, 0), AnchorPosition.TopCenter);


            txtbox = HUD.CreateWidget<WTextBox>(owningHUD);
            txtbox.SetGeometry(new Vector2(256, 32), AnchorPosition.TopCenter);
            txtbox.hintText = "IP-Adress";
            txtbox.AttachToParent(vb);


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

            hz.AttachToParent(vb, AnchorPosition.TopCenter);
        }

        private void BackPressed(ButtonPressArgs args)
        {
            var title = HUD.CreateWidget<UWTitle>(owningHUD);
            title.SetGeometry(new Vector2(1920, 1080), AnchorPosition.Center);
            DestroyWidget();
        }

        private void JoinPressed(DataStructures.ButtonPressArgs args)
        {
            if(Program.GetGame().localClient == null)
            {
                Program.GetGame().CreateNewClient();
            }
            var j =HUD.CreateWidget<UWJoinServer>(owningHUD);
            j.SetGeometry(new Vector2(1920, 1080), AnchorPosition.Center);

            Program.GetGame().JoinServer(txtbox.text);
            DestroyWidget();
        }
    }
}