using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.ID;

namespace Tiled
{
    public class Lighting
    {
        public const uint MAX_LIGHT = 32;
        public const uint MAX_SKY_LIGHT = 32;
        public static float SKY_LIGHT_MULT = 1.0f;
        private static HashSet<(int x, int y)> lightUpdateQueue = new HashSet<(int x, int y)>();

        public static void QueueLightUpdate(int x, int y)
        {
            if (!World.IsValidIndex(World.lightMap, x, y)) return;
            lightUpdateQueue.Add((x, y));

            // Queue neighbors too since they might need updates
            if (World.IsValidIndex(World.lightMap, x + 1, y)) lightUpdateQueue.Add((x + 1, y));
            if (World.IsValidIndex(World.lightMap, x - 1, y)) lightUpdateQueue.Add((x - 1, y));
            if (World.IsValidIndex(World.lightMap, x, y + 1)) lightUpdateQueue.Add((x, y + 1));
            if (World.IsValidIndex(World.lightMap, x, y - 1)) lightUpdateQueue.Add((x, y - 1));
        }

        public static void QueueGlobalLightUpdate()
        {
            // Assuming World.Width and World.Height are available
            // You might need to adjust this based on your chunk system
            for (int x = 0; x < World.maxTilesX; x++)
            {
                for (int y = 0; y < World.maxTilesY; y++)
                {
                    if (!World.IsValidTileOrWall(x, y))  // Only queue air tiles
                    {
                        QueueLightUpdate(x, y);
                    }
                }
            }
        }

        public static void ProcessLightUpdates()
        {
            if (lightUpdateQueue.Count == 0) return;

            Queue<(int x, int y)> propagationQueue = new Queue<(int x, int y)>();

            foreach (var pos in lightUpdateQueue)
            {
                uint oldLight = World.lightMap[pos.x, pos.y];
                uint newLight = CalculateLight(pos.x, pos.y);

                if (oldLight != newLight)
                {
                    World.lightMap[pos.x, pos.y] = newLight;
                    propagationQueue.Enqueue(pos);
                }
            }

            lightUpdateQueue.Clear();

            while (propagationQueue.Count > 0)
            {
                var (x, y) = propagationQueue.Dequeue();
                PropagateLight(x, y, propagationQueue);
            }
        }

        private static uint CalculateLight(int x, int y)
        {
            var tile = TileID.GetTile(World.tiles[x, y]);

            // Check if tile is a light source
            uint tileLight = tile.light;
            if (tileLight > 0) return tileLight;

            if (!World.IsValidTileOrWall(x, y))
            {
                uint skyLight = CalculateSkyLight(y);
                uint maxNeighborLight = GetMaxNeighborLight(x, y);
                return Math.Max(skyLight, maxNeighborLight > 0 ? maxNeighborLight - 2 : 0);
            }

            // For solid blocks, consider their light blocking property
            uint neighborLight = GetMaxNeighborLight(x, y);
            if (neighborLight == 0) return 0;

            // Calculate light reduction based on tile's blockLight property
            uint reduction = Math.Min(neighborLight, tile.blockLight);
            return neighborLight > reduction ? neighborLight - reduction : 0;
        }

        private static uint GetMaxNeighborLight(int x, int y)
        {
            uint maxLight = 0;
            var currentTile = TileID.GetTile(World.tiles[x, y]);

            // Check each neighbor, considering their blockLight property
            if (World.IsValidIndex(World.lightMap, x + 1, y))
            {
                var neighborTile = TileID.GetTile(World.tiles[x + 1, y]);
                uint light = World.lightMap[x + 1, y];
                if (light > neighborTile.blockLight)
                    maxLight = Math.Max(maxLight, light - neighborTile.blockLight);
            }
            if (World.IsValidIndex(World.lightMap, x - 1, y))
            {
                var neighborTile = TileID.GetTile(World.tiles[x - 1, y]);
                uint light = World.lightMap[x - 1, y];
                if (light > neighborTile.blockLight)
                    maxLight = Math.Max(maxLight, light - neighborTile.blockLight);
            }
            if (World.IsValidIndex(World.lightMap, x, y + 1))
            {
                var neighborTile = TileID.GetTile(World.tiles[x, y + 1]);
                uint light = World.lightMap[x, y + 1];
                if (light > neighborTile.blockLight)
                    maxLight = Math.Max(maxLight, light - neighborTile.blockLight);
            }
            if (World.IsValidIndex(World.lightMap, x, y - 1))
            {
                var neighborTile = TileID.GetTile(World.tiles[x, y - 1]);
                uint light = World.lightMap[x, y - 1];
                if (light > neighborTile.blockLight)
                    maxLight = Math.Max(maxLight, light - neighborTile.blockLight);
            }

            return maxLight;
        }

        private static uint CalculateSkyLight(int y)
        {
            return (uint)(MAX_SKY_LIGHT * SKY_LIGHT_MULT);
        }

        private static void PropagateLight(int x, int y, Queue<(int x, int y)> propagationQueue)
        {
            uint currentLight = World.lightMap[x, y];

            CheckNeighbor(x + 1, y, currentLight, propagationQueue);
            CheckNeighbor(x - 1, y, currentLight, propagationQueue);
            CheckNeighbor(x, y + 1, currentLight, propagationQueue);
            CheckNeighbor(x, y - 1, currentLight, propagationQueue);
        }

        private static void CheckNeighbor(int x, int y, uint sourceLight, Queue<(int x, int y)> propagationQueue)
        {
            if (!World.IsValidIndex(World.lightMap, x, y)) return;

            uint oldLight = World.lightMap[x, y];
            uint newLight = CalculateLight(x, y);

            if (oldLight != newLight)
            {
                World.lightMap[x, y] = newLight;
                propagationQueue.Enqueue((x, y));
            }
        }
    }
}