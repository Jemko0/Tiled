using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Tiled.DataStructures;
using Tiled.UI.Widgets;

namespace Tiled.UI.UserWidgets
{
    public class UWSettings : PanelWidget
    {
        public UWSettings(HUD owner) : base(owner)
        {
        }

        WVerticalBox vb;

        WButton backBtn;
        WText backBtnText;

        WText zoomSliderDesc;
        WSlider zoomSlider;
        WText zoomSliderText;

        WHorizontalBox hz;


        public const int vbWidth = 1024;
        public override void Construct()
        {
            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.childrenKeepWidth = false;
            vb.SetGeometry(new Vector2(vbWidth, 256), null);
            vb.AttachToParent(this, AnchorPosition.TopCenter);

            #region CameraZoom

            hz = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            hz.childrenKeepWidth = false;
            hz.AttachToParent(vb, AnchorPosition.TopCenter);

            zoomSliderDesc = HUD.CreateWidget<WText>(owningHUD);
            zoomSliderDesc.layerDepth = 1.0f;
            zoomSliderDesc.text = "Camera Zoom";
            zoomSliderDesc.AttachToParent(hz);

            zoomSlider = HUD.CreateWidget<WSlider>(owningHUD);
            zoomSlider.layerDepth = 1.0f;
            zoomSlider.minValue = 0.5f;
            zoomSlider.maxValue = 3.0f;
            zoomSlider.sliderValue = 1.0f;
            zoomSlider.maxDecimalPlaces = 2;
            zoomSlider.onSliderValueChanged += ZoomSlider_onSliderValueChanged;
            zoomSlider.AttachToParent(hz);

            zoomSliderText = HUD.CreateWidget<WText>(owningHUD);
            zoomSliderText.text = "Slider value";
            zoomSliderText.layerDepth = 1.0f;
            zoomSliderText.AttachToParent(hz);

            #endregion

            backBtn = HUD.CreateWidget<WButton>(owningHUD);
            backBtn.SetGeometry(new Vector2(128, 72), AnchorPosition.TopCenter);
            backBtn.layerDepth = 1.0f;
            backBtn.AttachToParent(vb);
            backBtn.onButtonPressed += BackButton_onButtonPressed;

            backBtnText = HUD.CreateWidget<WText>(owningHUD);
            backBtnText.SetGeometry(new Vector2(24, 24), AnchorPosition.TopCenter);
            backBtnText.text = "Back";
            backBtnText.justification = ETextJustification.Center;
            backBtnText.layerDepth = 0.9f;
            backBtnText.AttachToParent(backBtn);
        }

        private void BackButton_onButtonPressed(ButtonPressArgs args)
        {
            var t = HUD.CreateWidget<UWTitle>(owningHUD);
            t.SetGeometry(new(1920, 1080), AnchorPosition.Center);
            DestroyWidget();
        }

        private void ZoomSlider_onSliderValueChanged()
        {
            Program.GetGame().localCamera.zoom = zoomSlider.sliderValue;
            Program.GetGame().ForceCalcRenderScale();
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            SetGeometry(new Vector2(vbWidth, 0), DataStructures.AnchorPosition.Center);
            zoomSliderText.text = zoomSlider.sliderValue.ToString();
            base.DrawWidget(ref sb);
        }
    }
}
