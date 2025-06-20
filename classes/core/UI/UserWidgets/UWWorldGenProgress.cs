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
            vb.AttachToParent(this, DataStructures.AnchorPosition.Center);

            taskText = HUD.CreateWidget<WText>(owningHUD);
            taskText.SetGeometry(new Vector2(256, 72), null);
            taskText.justification = DataStructures.ETextJustification.Center;
            taskText.text = "taskText";
            taskText.AttachToParent(vb, DataStructures.AnchorPosition.Center);

            progressText = HUD.CreateWidget<WText>(owningHUD);
            progressText.SetGeometry(new Vector2(256, 72), null);
            progressText.justification = DataStructures.ETextJustification.Center;
            progressText.text = "progressText";
            progressText.AttachToParent(vb, DataStructures.AnchorPosition.Center);

            base.Construct();
            World.maxTilesX = 8400;
            World.maxTilesY = 500;
            Program.GetGame().world.seed = 555718233;

            Program.GetGame().world.StartWorldGeneration();
            Program.GetGame().world.taskProgressChanged += WGenProgressChanged;
            Program.GetGame().world.worldGenFinished += World_worldGenFinished;
        }

        private void World_worldGenFinished()
        {
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
