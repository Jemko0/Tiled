using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.DataStructures;

namespace Tiled.UI.UserWidgets
{
    public class UWMessage : PanelWidget
    {
        string reservedText;
        public UWMessage(HUD owner, string msg) : base(owner)
        {
            reservedText = msg;
        }

        WText t;
        WVerticalBox outer;
        WVerticalBox vb;
        Texture2D bgTex;
        WButton okBtn;
        WText okBtnTxt;

        Vector2 boxSize = new Vector2(1000, 960);
        public override void Construct()
        {
            layerDepth = 1.0f;

            SetGeometry(boxSize, DataStructures.AnchorPosition.Center);
            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.SetGeometry(boxSize, AnchorPosition.Center);
            vb.childrenKeepWidth = true;
            vb.AttachToParent(this);
            vb.SetOffset(new Vector2(0, 100f));

            t = HUD.CreateWidget<WText>(owningHUD);
            t.SetGeometry(new Vector2(180, 240), null);
            t.text = reservedText;
            t.justification = DataStructures.ETextJustification.Center;
            t.AttachToParent(vb);

            okBtn = HUD.CreateWidget<WButton>(owningHUD);
            okBtn.SetGeometry(new Vector2(180, 72), AnchorPosition.Center);
            okBtn.onButtonPressed += OkBtn_onButtonPressed;
            okBtn.AttachToParent(vb);

            okBtnTxt = HUD.CreateWidget<WText>(owningHUD);
            okBtnTxt.SetGeometry(new Vector2(180, 72), AnchorPosition.Center);
            okBtnTxt.justification = DataStructures.ETextJustification.Center;
            okBtnTxt.text = "OK";
            okBtnTxt.AttachToParent(okBtn);

            bgTex = Program.GetGame().Content.Load<Texture2D>("UI/button/btn-default");
            base.Construct();
        }

        private void OkBtn_onButtonPressed(DataStructures.ButtonPressArgs args)
        {
            DestroyWidget();
        }

        public override void DrawWidget(ref SpriteBatch sb)
        {
            sb.Draw(bgTex, scaledGeometry, Color.SteelBlue);
            base.DrawWidget(ref sb);
        }
    }
}