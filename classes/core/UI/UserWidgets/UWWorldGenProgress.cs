using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled.UI.UserWidgets
{
    public class UWWorldGenProgress : PanelWidget
    {
        public UWWorldGenProgress(HUD owner) : base(owner)
        {
        }

        WText taskText;
        WText progressText;

        WVerticalBox vb;
        public override void Construct()
        {
            SetGeometry(new Vector2(720, 256), DataStructures.AnchorPosition.Center);

            vb = HUD.CreateWidget<WVerticalBox>(owningHUD);
            vb.SetGeometry(new Vector2(720, 256), DataStructures.AnchorPosition.Center);
            vb.AttachToParent(this);

            taskText = HUD.CreateWidget<WText>(owningHUD);
            taskText.SetGeometry(new Vector2(0, 72), null);
            taskText.justification = DataStructures.ETextJustification.Center;
            taskText.text = "taskText";
            taskText.AttachToParent(vb);

            progressText = HUD.CreateWidget<WText>(owningHUD);
            progressText.SetGeometry(new Vector2(0, 72), null);
            progressText.justification = DataStructures.ETextJustification.Center;
            progressText.text = "progressText";
            progressText.AttachToParent(vb);

            base.Construct();
            World.maxTilesX = 1280;
            World.maxTilesY = 720;
            Program.GetGame().world.seed = 12345;

            Program.GetGame().world.StartWorldGeneration();
            Program.GetGame().world.taskProgressChanged += WGenProgressChanged;
            DestroyWidget();
        }

        // Synchronous entry point

        private void WGenProgressChanged(object sender, WorldGenProgress e)
        {
            taskText.text = e.CurrentTask.ToString();
            string t = Math.Ceiling(e.PercentComplete * 100f) + "%";
            progressText.text = t;
        }
    }
}
