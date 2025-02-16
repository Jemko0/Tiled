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

            StartWorldGeneration();
        }

        // Synchronous entry point
        public void StartWorldGeneration()
        {
            TaskCompletionSource<bool> _genTaskCompletionSource;
            Task _genTask;

            // Create a task completion source to track the overall process
            _genTaskCompletionSource = new TaskCompletionSource<bool>();

            // Start the async operation on a background thread
            _genTask = Task.Run(async () =>
            {
                try
                {
                    var newParams = new WorldGenParams()
                    {
                        maxTilesX = 5000,
                        maxTilesY = 5000,
                        seed = 0,
                    };

                    var game = Program.GetGame();
                    game.world.InitTasks();
                    game.world.taskProgressChanged += WGenProgressChanged;
                    World.renderWorld = false;

                    // Wait for the world generation to complete
                    await game.world.RunTasks(newParams);

                    // Only proceed if world generation was successful
                    if (game.world.LoadWorld(false))
                    {
                        game.CreatePlayer(new Vector2(newParams.maxTilesX / 2, 0));
                        World.renderWorld = true;
                        Main.inTitle = false;
                    }

                    DestroyWidget();
                    _genTaskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    _genTaskCompletionSource.SetException(ex);
                    throw;
                }
            });
        }

        private void WGenProgressChanged(object sender, WorldGenProgress e)
        {
            taskText.text = e.CurrentTask.ToString();
            string t = Math.Ceiling(e.PercentComplete * 100f) + "%";
            progressText.text = t;
        }
    }
}
