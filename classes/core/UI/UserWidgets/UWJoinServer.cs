using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;

namespace Tiled.UI.UserWidgets
{
    public class UWJoinServer : PanelWidget
    {
        public UWJoinServer(HUD owner) : base(owner)
        {
        }

        WText text;
        public override void Construct()
        {
            text = HUD.CreateWidget<WText>(owningHUD);
            
            text.text = "Joining Server...";
            text.justification = ETextJustification.Center;
            text.AttachToParent(this, AnchorPosition.Center);

            //Program.GetGame().localClient.OnException += LocalClient_OnException;
#if !TILEDSERVER
            Main.netClient.clientJoined += LocalClient_OnJoinResult;
            Main.netClient.clientException += LocalClient_OnException;
#endif
        }

        private void LocalClient_OnException(Exception e)
        {
            //HUD.CreateWidget<UWMessage>(owningHUD, e.Message);
            //Program.GetGame().localClient.DestroySocket();
            Debug.WriteLine("destroyed client socket cause of an exception!");
            DestroyWidget();
        }

        private void LocalClient_OnJoinResult(bool obj)
        {
            if(obj)
            {
                DestroyWidget();
            }
            else
            {
                HUD.CreateWidget<UWMessage>(owningHUD, "Failed to join server!");
                DestroyWidget();
            }
        }
#if !TILEDSERVER
        public override void DrawWidget(ref SpriteBatch sb)
        {
            text.text = "Joining Server..." + "\n" + "STATUS: " + Main.netClient.loadState.ToString();
            base.DrawWidget(ref sb);
        }
#endif
    }
}
