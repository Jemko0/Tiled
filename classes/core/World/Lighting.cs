using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Threading.Tasks;
using Tiled.ID;
using System.Linq;
using Tiled.Collision;
using Tiled.classes.core.Debug;

namespace Tiled
{
    public class Lighting
    {
        public const uint MAX_LIGHT = 32;
        public const uint MAX_SKY_LIGHT = 32;

        public static float skyLightMultiplier = 1.0f;
        private static readonly object queueLock = new object();
        private static HashSet<(int x, int y)> lightUpdateQueue = new HashSet<(int x, int y)>();
        public static bool isPerformingGlobalLightUpdate;

        public static Color skyBaseColor = new Color();
        public static Color skyHorizonColor = new Color(255, 255, 255, 255);

        // Configuration for chunk processing
        private const int CHUNK_SIZE = 32;
        private const int UPDATES_PER_FRAME = 4;
        private static Queue<(int startX, int startY, int endX, int endY)> chunksToProcess = new Queue<(int startX, int startY, int endX, int endY)>();

        // Task management
        private static CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private static Task currentProcessingTask = null;

        public static void QueueLightUpdate(int x, int y)
        {
            if (!World.IsValidIndex(World.lightMap, x, y)) return;

            lock (queueLock)
            {
                lightUpdateQueue.Add((x, y));

                // Queue neighbors too since they might need updates
                if (World.IsValidIndex(World.lightMap, x + 1, y)) lightUpdateQueue.Add((x + 1, y));
                if (World.IsValidIndex(World.lightMap, x - 1, y)) lightUpdateQueue.Add((x - 1, y));
                if (World.IsValidIndex(World.lightMap, x, y + 1)) lightUpdateQueue.Add((x, y + 1));
                if (World.IsValidIndex(World.lightMap, x, y - 1)) lightUpdateQueue.Add((x, y - 1));
            }
        }

        public static void QueueGlobalLightUpdate()
        {
            // Cancel any existing processing
            cancellationSource.Cancel();
            cancellationSource = new CancellationTokenSource();

            lock (queueLock)
            {
                isPerformingGlobalLightUpdate = true;
                chunksToProcess.Clear();
                lightUpdateQueue.Clear();

                // Divide the world into chunks
                for (int x = 0; x < World.maxTilesX; x += CHUNK_SIZE)
                {
                    for (int y = 0; y < World.maxTilesY; y += CHUNK_SIZE)
                    {
                        int endX = Math.Min(x + CHUNK_SIZE, World.maxTilesX);
                        int endY = Math.Min(y + CHUNK_SIZE, World.maxTilesY);
                        chunksToProcess.Enqueue((x, y, endX, endY));
                    }
                }
            }

            // Start processing in background
            StartBackgroundProcessing();
        }

        private static void StartBackgroundProcessing()
        {
            Benchmark.StartBenchmark("Lighting: Background Processing");
            var token = cancellationSource.Token;
            currentProcessingTask = Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        bool hasChunks;
                        lock (queueLock)
                        {
                            hasChunks = chunksToProcess.Count > 0;
                        }

                        if (!hasChunks) break;

                        for (int i = 0; i < UPDATES_PER_FRAME; i++)
                        {
                            (int startX, int startY, int endX, int endY) chunk;
                            lock (queueLock)
                            {
                                if (chunksToProcess.Count == 0) break;
                                chunk = chunksToProcess.Dequeue();
                            }
                            await ProcessChunkAsync(chunk.startX, chunk.startY, chunk.endX, chunk.endY, token);
                        }

                        await Task.Delay(1, token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled, just exit
                }
                finally
                {
                    lock (queueLock)
                    {
                        isPerformingGlobalLightUpdate = false;
                    }
                }
            }, token);
            Benchmark.EndBenchmark("Lighting: Background Processing");
        }

        // Called from your synchronous update method
        public static void Update()
        {
            Benchmark.StartBenchmark("Lighting: Update");
            List<(int x, int y)> updates;
            lock (queueLock)
            {
                if (lightUpdateQueue.Count == 0) return;
                updates = new List<(int x, int y)>(lightUpdateQueue);
                lightUpdateQueue.Clear();
            }

            if (updates.Count > 0)
            {
                Task.Run(() => ProcessLightUpdatesAsync(updates, cancellationSource.Token));
            }
            Benchmark.EndBenchmark("Lighting: Update");
        }

        private static async Task ProcessChunkAsync(int startX, int startY, int endX, int endY, CancellationToken token)
        {
            Benchmark.StartBenchmark("Lighting: ProcessChunksAsync");
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if (token.IsCancellationRequested) return;

                    if (!World.IsValidTileOrWall(x, y))
                    {
                        QueueLightUpdate(x, y);
                    }
                }
            }
            Benchmark.EndBenchmark("Lighting: ProcessChunksAsync");
        }

        private static async Task ProcessLightUpdatesAsync(List<(int x, int y)> positions, CancellationToken token)
        {
            Benchmark.StartBenchmark("Lighting: ProcessLightUpdatesAsync");
            Queue<(int x, int y)> propagationQueue = new Queue<(int x, int y)>();

            foreach (var pos in positions)
            {
                if (token.IsCancellationRequested) return;

                uint oldLight = World.lightMap[pos.x, pos.y];
                uint newLight = CalculateLight(pos.x, pos.y);

                if (oldLight != newLight)
                {
                    World.lightMap[pos.x, pos.y] = newLight;
                    propagationQueue.Enqueue(pos);
                }
            }

            while (propagationQueue.Count > 0)
            {
                if (token.IsCancellationRequested) return;

                var (x, y) = propagationQueue.Dequeue();
                PropagateLight(x, y, propagationQueue);
            }
            Benchmark.EndBenchmark("Lighting: ProcessLightUpdatesAsync");
        }

        private static uint CalculateLight(int x, int y)
        {
            /*var entity = CollisionStatics.isEntityWithinRect(new System.Drawing.RectangleF(x * World.TILESIZE, y * World.TILESIZE, World.TILESIZE, World.TILESIZE));

            if(entity != null)
            {
                return entity.light;
            }*/
            
            var tile = TileID.GetTile(World.tiles[x, y]);

            // Check if tile is a light source
            uint tileLight = tile.light;
            if (tileLight > 0) return tileLight;

            if (!World.IsValidTileOrWall(x, y))
            {
                uint skyLight = CalculateSkyLight(y);
                uint maxNeighborLight = GetMaxNeighborLight(x, y);
                return Math.Max(skyLight, maxNeighborLight > 0 ? maxNeighborLight - 1 : 0);
            }

            uint neighborLight = GetMaxNeighborLight(x, y);

            if (neighborLight == 0)
            {
                return 0;
            }

            uint reduction = Math.Min(neighborLight, tile.blockLight);
            return neighborLight > reduction ? neighborLight - reduction : 0;
        }

        private static uint GetMaxNeighborLight(int x, int y)
        {
            uint maxLight = 0;

            if (World.IsValidIndex(World.lightMap, x + 1, y))
            {
                maxLight = Math.Max(maxLight, World.lightMap[x + 1, y]);
            }
            if (World.IsValidIndex(World.lightMap, x - 1, y))
            {
                maxLight = Math.Max(maxLight, World.lightMap[x - 1, y]);
            }
            if (World.IsValidIndex(World.lightMap, x, y + 1))
            {
                maxLight = Math.Max(maxLight, World.lightMap[x, y + 1]);
            }
            if (World.IsValidIndex(World.lightMap, x, y - 1))
            {
                maxLight = Math.Max(maxLight, World.lightMap[x, y - 1]);
            }

            return maxLight;
        }
        
        public static uint CalculateSkyLight(int y)
        {
            if(y < World.averageSurfaceHeight + 80)
            {
                return (uint)(MAX_SKY_LIGHT * skyLightMultiplier);
            }
            return 0;
        }

        public static float GetInterpLight(float x, float y)
        {
            
            int centerX = (int)x;
            int centerY = (int)y;

            float[] neighbors = new float[4]
            {
                World.IsValidIndex(World.lightMap, centerX + 1, centerY)? World.lightMap[centerX + 1, centerY] : 0.0f,
                World.IsValidIndex(World.lightMap, centerX - 1, centerY)? World.lightMap[centerX - 1, centerY] : 0.0f,
                World.IsValidIndex(World.lightMap, centerX, centerY - 1)? World.lightMap[centerX, centerY - 1] : 0.0f,
                World.IsValidIndex(World.lightMap, centerX, centerY + 1)? World.lightMap[centerX, centerY + 1] : 0.0f,
            };

            //average the neighbor array
            float sum = neighbors[0] + neighbors[1] + neighbors[2] + neighbors[3];
            float avg = sum / 4.0f;

            return avg;
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