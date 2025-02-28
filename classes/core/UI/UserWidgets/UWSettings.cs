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
        WSlider zoomSlider;
        WText zoomSliderText;

        public const int vbWidth = 256;
        public override void Construct()
        {
            mainVert = HUD.CreateWidget<WVerticalBox>(owningHUD);
            mainVert.SetGeometry(new Vector2(vbWidth, 256), AnchorPosition.Center);
            mainVert.AttachToParent(this);

            //camZoomSlider
            WHorizontalBox s1 = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            s1.SetGeometry(new Vector2(512, 32), AnchorPosition.Center);
            s1.AttachToParent(mainVert);

            zoomSlider = HUD.CreateWidget<WSlider>(owningHUD);
            zoomSlider.minValue = 0.5f;
            zoomSlider.maxValue = 3.0f;
            zoomSlider.maxDecimalPlaces = 2;
            zoomSlider.SetGeometry(new Vector2(512, 16), AnchorPosition.Center);
            zoomSlider.AttachToParent(s1);
            zoomSlider.onSliderValueChanged += ZoomSlider_onSliderValueChanged;

            zoomSliderText = HUD.CreateWidget<WText>(owningHUD);
            zoomSliderText.SetGeometry(new Vector2(128, 16), AnchorPosition.Center);
            zoomSliderText.justification = ETextJustification.Center;
            zoomSliderText.AttachToParent(s1);

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
            Main.userSettings.data.camZoom = zoomSlider.sliderValue;
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            SetGeometry(new Vector2(vbWidth, 0), DataStructures.AnchorPosition.Center);
            zoomSliderText.text = zoomSlider.sliderValue.ToString();
            base.DrawWidget(ref sb);
        }
    }
}
