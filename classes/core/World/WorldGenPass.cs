using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled
{
    public class WorldGenParams
    {
        public int seed { get; set; }
        public int maxTilesX { get; set; }
        public int maxTilesY { get; set; }
    }
    public class WorldGenProgress
    {
        public float PercentComplete { get; set; }
        public string CurrentTask { get; set; }
        public bool isCompleted { get; set; }
    }
    
    public class WorldGenTask
    {
        public string taskName { get; set; }
        public WorldGenTask(string identifier)
        {
            taskName = identifier;
        }
    
        public virtual void InitTask()
        {
    
        }
    
        public virtual async Task Run(IProgress<WorldGenProgress> progress, WorldGenParams wparams)
        {
    
        }
    }

    public class WGT_TestFillWorld : WorldGenTask
    {
        public WGT_TestFillWorld(string identifier) : base(identifier)
        {
        }

        public override async Task Run(IProgress<WorldGenProgress> progress, WorldGenParams wparams)
        {
            for(int x = 0; x < wparams.maxTilesX; x++)
            {
                for (int y = 0; y < wparams.maxTilesX; y++)
                {
                    if(y > 80)
                    {
                        World.tiles[x, y] = DataStructures.ETileType.Dirt;
                        World.walls[x, y] = DataStructures.EWallType.Dirt;
                    }
                }

                progress?.Report(new WorldGenProgress()
                {
                    CurrentTask = "Test Filling World",
                    PercentComplete = (float)x / wparams.maxTilesX,
                });
            }

            World.CompleteCurrent();
        }
    }
}
