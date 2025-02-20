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
            text.AttachToParent(this, AnchorPosition.Center);

            Program.GetGame().localClient.OnException += LocalClient_OnException;
            Program.GetGame().localClient.OnJoinResult += LocalClient_OnJoinResult;
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
            }
        }

        private void LocalClient_OnException(string obj)
        {
            HUD.CreateWidget<UWMessage>(owningHUD, obj);
            Program.GetGame().localClient.DestroySocket();
            Debug.WriteLine("destroyed client socket cause of an exception!");
        }
    }
}
