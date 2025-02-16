using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        WVerticalBox vb;
        Texture2D bgTex;
        WButton okBtn;
        WText okBtnTxt;
        public override void Construct()
        {
            SetGeometry(new Vector2(720, 480), DataStructures.AnchorPosition.Center);

            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.SetGeometry(new Vector2(720, 240), DataStructures.AnchorPosition.Center);
            vb.AttachToParent(this);
            vb.SetOffset(new Vector2(0, 100f));

            t = HUD.CreateWidget<WText>(owningHUD);
            t.SetGeometry(new Vector2(0, 240), null);
            t.text = reservedText;
            t.justification = DataStructures.ETextJustification.Center;
            t.AttachToParent(vb);

            okBtn = HUD.CreateWidget<WButton>(owningHUD);
            okBtn.SetGeometry(new Vector2(0, 72), null);
            okBtn.onButtonPressed += OkBtn_onButtonPressed;
            okBtn.AttachToParent(vb);

            okBtnTxt = HUD.CreateWidget<WText>(owningHUD);
            okBtnTxt.SetGeometry(new Vector2(0, 72), null);
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
