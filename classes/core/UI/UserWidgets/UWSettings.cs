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

        WVerticalBox mainVert;

        WButton backButton;
        WText backButtonText;

        WText zoomSliderDesc;
        WSlider zoomSlider;
        WText zoomSliderText;

        public const int width = 1024;
        public override void Construct()
        {
            mainVert = HUD.CreateWidget<WVerticalBox>(owningHUD);
            mainVert.SetGeometry(new Vector2(width, 256), AnchorPosition.Center);
            mainVert.childrenKeepWidth = true;
            mainVert.AttachToParent(this);

            //camZoomSlider
            WHorizontalBox h1 = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            h1.SetGeometry(new Vector2(512, 128), AnchorPosition.Center);
            h1.AttachToParent(mainVert);
            //h1.childrenAnchorOffset = new Vector2(0.5f, 0.5f);

            zoomSliderDesc = HUD.CreateWidget<WText>(owningHUD);
            zoomSliderDesc.SetGeometry(new Vector2(150, 16), AnchorPosition.Center);
            zoomSliderDesc.text = "Camera Zoom";
            zoomSliderDesc.layerDepth = 1.0f;
            zoomSliderDesc.autoSize = true;
            zoomSliderDesc.justification = ETextJustification.Center;
            zoomSliderDesc.AttachToParent(h1);

            /*zoomSlider = HUD.CreateWidget<WSlider>(owningHUD);
            zoomSlider.minValue = 0.5f;
            zoomSlider.maxValue = 3.0f;
            zoomSlider.maxDecimalPlaces = 2;
            zoomSlider.layerDepth = 0.9f;
            zoomSlider.SetGeometry(new Vector2(512, 16), AnchorPosition.Center);
            zoomSlider.sliderValue = 1.0f;
            zoomSlider.onSliderValueChanged += ZoomSlider_onSliderValueChanged;
            zoomSlider.AttachToParent(h1);*/

            zoomSliderText = HUD.CreateWidget<WText>(owningHUD);
            zoomSliderText.SetGeometry(new Vector2(48, 16), AnchorPosition.Center);
            zoomSliderText.justification = ETextJustification.Center;
            zoomSliderText.layerDepth = 1.0f;
            zoomSliderText.autoSize = true;
            zoomSliderText.AttachToParent(h1);

            
            //back
            backButton = HUD.CreateWidget<WButton>(owningHUD);
            backButton.SetGeometry(new Vector2(128, 72), AnchorPosition.Center);
            backButton.AttachToParent(mainVert);
            backButton.onButtonPressed += BackButton_onButtonPressed;

            backButtonText = HUD.CreateWidget<WText>(owningHUD);
            backButtonText.text = "Back";
            backButtonText.justification = ETextJustification.Center;
            backButtonText.AttachToParent(backButton);
        }

        private void BackButton_onButtonPressed(ButtonPressArgs args)
        {
            var t = HUD.CreateWidget<UWTitle>(owningHUD);
            t.SetGeometry(new(1920, 1080), AnchorPosition.Center);
            DestroyWidget();
        }

        private void ZoomSlider_onSliderValueChanged()
        {
            //Main.userSettings.data.camZoom = zoomSlider.sliderValue;
            Program.GetGame().localCamera.zoom = zoomSlider.sliderValue;
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            SetGeometry(new Vector2(width, 0), DataStructures.AnchorPosition.Center);
            /*zoomSliderText.text = zoomSlider.sliderValue.ToString();*/
            base.DrawWidget(ref sb);
        }
    }
}
