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
        WSlider zoomSlider;
        WText zoomSliderText;


        public override void Construct()
        {
            mainVert = HUD.CreateWidget<WVerticalBox>(owningHUD);
            mainVert.SetGeometry(new Vector2(512, 720), AnchorPosition.Center);
            mainVert.AttachToParent(this);

            //settings

            WHorizontalBox s1 = HUD.CreateWidget<WHorizontalBox>(owningHUD);
            s1.SetGeometry(new Vector2(512, 720), AnchorPosition.Center);
            s1.AttachToParent(mainVert);

            zoomSlider = HUD.CreateWidget<WSlider>(owningHUD);
            zoomSlider.minValue = 0.5f;
            zoomSlider.maxValue = 3.0f;
            zoomSlider.maxDecimalPlaces = 2;
            zoomSlider.SetGeometry(new Vector2(512, 16), AnchorPosition.Center);
            zoomSlider.AttachToParent(s1);

            zoomSliderText = HUD.CreateWidget<WText>(owningHUD);
            zoomSliderText.SetGeometry(new Vector2(128, 16), AnchorPosition.Center);
            zoomSliderText.justification = ETextJustification.Center;
            zoomSliderText.AttachToParent(s1);
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            zoomSliderText.text = zoomSlider.sliderValue.ToString();

            base.DrawWidget(ref sb);
        }
    }
}
