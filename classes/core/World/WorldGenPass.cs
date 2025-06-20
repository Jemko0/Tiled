using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tiled.DataStructures;

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
                        World.tiles[x, y] = DataStructures.ETileType.Stone;
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

    public class WGT_Terrain : WorldGenTask
    {
        public WGT_Terrain(string identifier) : base(identifier)
        {
        }

        public override async Task Run(IProgress<WorldGenProgress> progress, WorldGenParams wparams)
        {
            using var cts = new CancellationTokenSource();

            await Task.Run(() =>
            {
                FastNoiseLite dirtNoise = new FastNoiseLite();
                dirtNoise.SetSeed(wparams.seed);
                dirtNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
                dirtNoise.SetFractalOctaves(3);

                FastNoiseLite noise = new FastNoiseLite();
                int baseSurfaceHeight = wparams.maxTilesY / 2;

                World.cavernsLayerHeight = wparams.maxTilesY - wparams.maxTilesY / 8;

                const int CHUNK_SIZE = 100;
                for (int chunkStart = 0; chunkStart < wparams.maxTilesX; chunkStart += CHUNK_SIZE)
                {
                    int chunkEnd = Math.Min(chunkStart + CHUNK_SIZE, wparams.maxTilesX);

                    // Process each chunk
                    for (int x = chunkStart; x < chunkEnd; x++)
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;

                        int surfaceTileY = CalcSurface(x, wparams, noise, baseSurfaceHeight);
                        int rockHeight = surfaceTileY + 30;
                        World.surfaceHeights[x] = surfaceTileY;

                        for (int y = surfaceTileY; y < wparams.maxTilesY; y++)
                        {
                            ETileType placeType = ETileType.Air;
                            dirtNoise.SetFrequency(MathHelper.Lerp(0.01f, 0.15f, (y / (float)wparams.maxTilesY)));

                            if (y == surfaceTileY)
                            {
                                placeType = ETileType.Grass;
                            }
                            else
                            {
                                if(y > rockHeight)
                                {
                                    
                                    if (dirtNoise.GetNoise(x, y) > -0.4f + (y / (float)wparams.maxTilesY))
                                    {
                                        placeType = ETileType.Dirt;
                                    }
                                    else
                                    {
                                        placeType = ETileType.Stone;
                                    }
                                }
                                else
                                {
                                    placeType = ETileType.Dirt;
                                }
                            }
                            //World.walls[x, y] = EWallType.Dirt;
                            if(World.IsValidIndex(World.tiles, x, y))
                            {
                                World.tiles[x, y] = placeType;
                            }
                        }
                    }

                    // Report progress after each chunk
                    float percentComplete = (float)chunkEnd / wparams.maxTilesX;
                    progress?.Report(new WorldGenProgress
                    {
                        CurrentTask = "Terrain",
                        PercentComplete = percentComplete
                    });
                }

                World.averageSurfaceHeight = (int)World.surfaceHeights.Average();

                World.CompleteCurrent();
            }, cts.Token);
        }

        private static int CalcSurface(int x, WorldGenParams wparams, FastNoiseLite noise, int baseSurface)
        {
            //noise1
            noise.SetSeed(wparams.seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(3);
            noise.SetFractalLacunarity(1.819654321f);
            noise.SetFrequency(0.01345f);
            float noise1 = (noise.GetNoise(x, 0)) * 15.11f;

            noise.SetSeed(wparams.seed + 41 * 3);
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
            noise.SetFractalOctaves(2);
            noise.SetFractalLacunarity(1.314614361f);
            noise.SetFrequency(0.02345f);
            float noise2 = (noise.GetNoise(x, 0)) * 4f;

            noise.SetSeed(wparams.seed + 31 * 3);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(1);
            noise.SetFractalLacunarity(1.1f);
            noise.SetFrequency(0.0135f);
            float pv = (noise.GetNoise(x, 0)) * 15.0f;

            float finalVal = (noise1 - noise2) - pv;
            return baseSurface + (int)finalVal;
        }
    }

    public class WGT_PlaceTrees : WorldGenTask
    {
        public WGT_PlaceTrees(string identifier) : base(identifier)
        {
        }

        public override async Task Run(IProgress<WorldGenProgress> progress, WorldGenParams wparams)
        {
            using var cts = new CancellationTokenSource();

            await Task.Run(() =>
            {
                for(int x = 4; x < World.surfaceHeights.Length - 4; x += 4)
                {
                    int treeX = x;
                    int treeY = World.surfaceHeights[x] - 1;

                    if
                    (
                    (World.tiles[treeX, treeY + 1] == ETileType.Grass || World.tiles[treeX, treeY + 1] == ETileType.Dirt)
                    &&
                    (World.tiles[treeX + 1, treeY + 1] == ETileType.Grass || World.tiles[treeX + 1, treeY + 1] == ETileType.Dirt)
                    &&
                    (World.tiles[treeX - 1, treeY + 1] == ETileType.Grass || World.tiles[treeX - 1, treeY + 1] == ETileType.Dirt)
                    )
                    {
                        if (World.tiles[treeX + 1, treeY] == ETileType.Air && World.tiles[treeX - 1, treeY] == ETileType.Air)
                        {
                            PlaceTree(treeX, treeY, wparams);
                        }
                    }

                    progress?.Report(new WorldGenProgress
                    {
                        CurrentTask = "Placing Trees",
                        PercentComplete = (float)x / World.surfaceHeights.Length
                    });
                }

                World.CompleteCurrent();
            }, cts.Token);
        }

        public static void PlaceTree(int x, int y, WorldGenParams wp)
        {
            World.SetTile(x, y, ETileType.TreeTrunk);
            World.SetTile(x + 1, y, ETileType.TreeTrunk);
            World.SetTile(x - 1, y, ETileType.TreeTrunk);

            int minTreeLength = 4;
            int maxTreeLength = 20;
            int highestTreeTile = 0;
            int treeLength = Math.Clamp((int)((new Random(wp.seed + x + y).NextSingle()) * maxTreeLength), minTreeLength, int.MaxValue);

            for(int trunkY = 0; trunkY < treeLength; trunkY++)
            {
                World.SetTile(x, y - trunkY, ETileType.TreeTrunk);
                highestTreeTile = y - trunkY;
            }

            //LEAVES
            float exp = 1.5f;

            int minRad = (int)Math.Clamp(Math.Pow(treeLength / 3, exp), 3, int.MaxValue);
            int maxRad = (int)Math.Pow(treeLength / 2, exp) / 2;
            int radius = Math.Clamp((int)((new Random(wp.seed - 51 + x + y + wp.seed - 112).NextSingle()) * maxRad), minRad, int.MaxValue);

            int centerX = x;
            int treeTop = highestTreeTile - radius;


            Random random = new Random();

            for (int ly = treeTop; ly <= treeTop + radius; ly++)
            {
                int distanceFromTop = ly - treeTop;

                double heightProgress = (double)distanceFromTop / radius;
                int levelRadius;

                if (heightProgress < 0.2)
                {
                    levelRadius = (int)(radius * heightProgress * 2.5);
                }
                else if (heightProgress < 0.8)
                {
                    levelRadius = (int)(radius * 0.8);
                }
                else
                {
                    levelRadius = (int)(radius * (1 - (heightProgress - 0.8) * 2.5));
                }

                levelRadius = Math.Max(1, levelRadius);

                int levelMinX = Math.Max(0, centerX - levelRadius);
                int levelMaxX = Math.Min(World.tiles.GetLength(0) - 1, centerX + levelRadius);

                for (int lx = levelMinX; lx <= levelMaxX; lx++)
                {
                    double distanceFromCenter = Math.Abs(lx - centerX);

                    if (distanceFromCenter <= levelRadius * (0.9 + random.NextDouble() * 0.2))
                    {

                        if (!World.IsValidTile(lx, ly) && ly >= 0 && ly < World.tiles.GetLength(1))
                        {
                            World.SetTile(lx, ly, ETileType.TreeLeaves);
                        }
                    }
                }
            }
        }
    }

    public class WGT_Caves : WorldGenTask
    {
        public WGT_Caves(string identifier) : base(identifier)
        {
        }

        private void GenerateConnectingTunnel(int startX, int startY, int endX, int endY)
        {
            int currentX = startX;
            int currentY = startY;
            int tunnelHeight = 4;
            int wallThickness = 3;
            int maxIterations = 100; // Safety limit
            int currentIteration = 0;
            
            // Calculate overall direction for natural curve tendency
            float overallAngle = (float)Math.Atan2(endY - startY, endX - startX);
            Random random = new Random(startX * 1000 + startY + endX * 100 + endY);

            while ((Math.Abs(currentX - endX) > 5 || Math.Abs(currentY - endY) > 5) && currentIteration < maxIterations)
            {
                currentIteration++;
                
                // Add some randomness to the path
                float noise = (float)(random.NextDouble() - 0.5) * 0.3f; // Reduced noise
                float currentAngle = overallAngle + noise;
                
                // Calculate direction with noise
                int directionX = (int)Math.Round(Math.Cos(currentAngle) * 3); // Reduced step size
                int directionY = (int)Math.Round(Math.Sin(currentAngle) * 3);
                
                // Ensure we're making progress
                if (directionX == 0 && Math.Abs(currentX - endX) > 5)
                {
                    directionX = currentX < endX ? 1 : -1;
                }
                if (directionY == 0 && Math.Abs(currentY - endY) > 5)
                {
                    directionY = currentY < endY ? 1 : -1;
                }

                int segmentLength = random.Next(5, 10); // Shorter segments
                
                // First pass: Place walls in a thick area
                for (int i = 0; i < segmentLength; i++)
                {
                    int digX = currentX + (i * directionX);
                    int digY = currentY + (i * directionY);

                    // Create a wider area of walls around the tunnel
                    for (int w = -wallThickness; w <= wallThickness; w++)
                    {
                        for (int h = -wallThickness; h <= tunnelHeight + wallThickness; h++)
                        {
                            int wallX = digX + w;
                            int wallY = digY + h;
                            if (World.IsValidTile(wallX, wallY))
                            {
                                World.SetWall(wallX, wallY, EWallType.Dirt);
                                // Add stone at the edges for support
                                if (Math.Abs(w) >= wallThickness - 1 || h <= -wallThickness + 1 || h >= tunnelHeight + wallThickness - 1)
                                {
                                    World.SetTile(wallX, wallY, ETileType.Stone);
                                }
                            }
                        }
                    }
                }

                // Second pass: Carve out the tunnel
                for (int i = 0; i < segmentLength; i++)
                {
                    int digX = currentX + (i * directionX);
                    int digY = currentY + (i * directionY);

                    // Create the main tunnel space
                    for (int w = -2; w <= 2; w++)
                    {
                        for (int h = 0; h < tunnelHeight; h++)
                        {
                            int tunnelX = digX + w;
                            int tunnelY = digY + h;
                            if (World.IsValidTile(tunnelX, tunnelY))
                            {
                                World.SetTile(tunnelX, tunnelY, ETileType.Air);
                                World.SetWall(tunnelX, tunnelY, EWallType.Dirt);
                            }
                        }
                    }
                }

                // Update position
                currentX += directionX * segmentLength;
                currentY += directionY * segmentLength;

                // Reduced random deviation
                if (random.NextDouble() < 0.2)
                {
                    currentY += random.Next(-1, 2);
                }
                if (random.NextDouble() < 0.2)
                {
                    currentX += random.Next(-1, 2);
                }
            }
        }

        public override async Task Run(IProgress<WorldGenProgress> progress, WorldGenParams wparams)
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(2)); // Safety timeout

            try
            {
                await Task.Run(() =>
                {
                    // Store entrance points for later connection
                    List<(int x, int y)> entranceEndPoints = new List<(int x, int y)>();
                    
                    // Calculate the depth range for caves
                    int maxDepth = wparams.maxTilesY - World.averageSurfaceHeight;
                    int minDepth = 30;
                    int safeSurfaceDepth = 50;
                    
                    // First pass: Generate underground caves
                    for(int x = 0; x < World.surfaceHeights.Length; x += 15) // Increased spacing
                    {
                        if (cts.Token.IsCancellationRequested) return;

                        float depthFactor = (float)Math.Pow(new Random(wparams.seed + x).NextDouble(), 1.5f);
                        int randomDepth = minDepth + (int)(depthFactor * (maxDepth - minDepth));
                        int startY = World.surfaceHeights[x] + randomDepth;
                        
                        if (startY - World.surfaceHeights[x] < safeSurfaceDepth && new Random(wparams.seed + x).NextDouble() > 0.3f)
                        {
                            continue;
                        }
                        
                        float depthProgress = (float)(startY - World.surfaceHeights[x]) / maxDepth;
                        
                        // Adjust cave parameters based on depth
                        int baseLength = 12;
                        int baseWidth = 80;
                        
                        // Make surface caves more tunnel-like
                        if (startY - World.surfaceHeights[x] < 100)
                        {
                            baseWidth = 40; // Narrower for surface caves
                            baseLength = 20; // Longer for surface caves
                        }
                        
                        int len = (int)(new Random(wparams.seed - x + startY).NextSingle() * (baseLength + depthProgress * 15.0f));
                        int w = (int)(new Random(wparams.seed / (x + 1) + len).NextSingle() * (baseWidth + depthProgress * 40.0f));

                        World.GenerateCaveSystem(x, startY, len, w);
                        progress.Report(new WorldGenProgress { CurrentTask = "Caves", PercentComplete = (float)x / World.surfaceHeights.Length });
                    }

                    // Second pass: Generate walkable cave entrances with pronounced zig-zags
                    for(int x = 0; x < World.surfaceHeights.Length; x += 50) // Increased spacing
                    {
                        if (cts.Token.IsCancellationRequested) return;

                        if (new Random(wparams.seed + x + 1000).NextDouble() > 0.2f) // Reduced frequency
                        {
                            continue;
                        }

                        int entranceX = x;
                        int entranceY = World.surfaceHeights[entranceX];

                        // Check for suitable terrain
                        bool suitableSpot = true;
                        for (int checkX = entranceX - 2; checkX <= entranceX + 2; checkX++)
                        {
                            if (checkX < 0 || checkX >= World.surfaceHeights.Length || 
                                Math.Abs(World.surfaceHeights[checkX] - entranceY) > 2)
                            {
                                suitableSpot = false;
                                break;
                            }
                        }

                        if (!suitableSpot) continue;

                        // Generate entrance chamber
                        int entranceWidth = 4;
                        int entranceHeight = 3;
                        int wallThickness = 3;

                        // First pass: Fill the entire area with stone
                        for (int dx = -entranceWidth/2 - wallThickness; dx <= entranceWidth/2 + wallThickness; dx++)
                        {
                            for (int dy = -wallThickness; dy < entranceHeight + wallThickness; dy++)
                            {
                                int digX = entranceX + dx;
                                int digY = entranceY + dy;
                                if (World.IsValidTile(digX, digY))
                                {
                                    World.SetTile(digX, digY, ETileType.Stone);
                                    World.SetWall(digX, digY, EWallType.Dirt);
                                }
                            }
                        }

                        // Second pass: Place additional walls in a thick border
                        for (int dx = -entranceWidth/2 - wallThickness; dx <= entranceWidth/2 + wallThickness; dx++)
                        {
                            for (int dy = -wallThickness; dy < entranceHeight + wallThickness; dy++)
                            {
                                int digX = entranceX + dx;
                                int digY = entranceY + dy;
                                
                                // Check if this is part of the border
                                bool isBorder = Math.Abs(dx) >= entranceWidth/2 + wallThickness - 1 || 
                                              dy <= -wallThickness + 1 ||
                                              dy >= entranceHeight + wallThickness - 1;
                                
                                if (World.IsValidTile(digX, digY) && isBorder)
                                {
                                    // Ensure walls are placed in border area
                                    World.SetWall(digX, digY, EWallType.Dirt);
                                    World.SetTile(digX, digY, ETileType.Stone);
                                }
                            }
                        }

                        // Third pass: Carve out the actual chamber space
                        for (int dx = -entranceWidth/2; dx <= entranceWidth/2; dx++)
                        {
                            for (int dy = 0; dy < entranceHeight; dy++)
                            {
                                int digX = entranceX + dx;
                                int digY = entranceY + dy;
                                if (World.IsValidTile(digX, digY))
                                {
                                    World.SetTile(digX, digY, ETileType.Air);
                                    // Keep the walls in place
                                    World.SetWall(digX, digY, EWallType.Dirt);
                                }
                            }
                        }

                        // Fourth pass: Ensure walls at the edges of the chamber
                        for (int dx = -entranceWidth/2 - 1; dx <= entranceWidth/2 + 1; dx++)
                        {
                            for (int dy = -1; dy <= entranceHeight; dy++)
                            {
                                int digX = entranceX + dx;
                                int digY = entranceY + dy;
                                
                                // Check if this is the edge of the chamber
                                bool isEdge = Math.Abs(dx) == entranceWidth/2 + 1 || 
                                            dy == -1 ||
                                            dy == entranceHeight;
                                
                                if (World.IsValidTile(digX, digY) && isEdge)
                                {
                                    World.SetWall(digX, digY, EWallType.Dirt);
                                    World.SetTile(digX, digY, ETileType.Stone);
                                }
                            }
                        }

                        // Generate more pronounced zig-zag entrance tunnel
                        int tunnelLength = 120 + (int)(new Random(wparams.seed + x).NextSingle() * 60);
                        int tunnelHeight = 4; // Increased height
                        int currentX = entranceX;
                        int currentY = entranceY + entranceHeight;
                        bool goingRight = new Random(wparams.seed + x).NextDouble() > 0.5f;
                        int horizontalLength = 35; // Much longer horizontal segments
                        float slopeRatio = 0.6f; // Steeper slope

                        int segmentCount = 0;
                        while (segmentCount * horizontalLength < tunnelLength)
                        {
                            // Create sloped segment
                            for (int dx = 0; dx < horizontalLength; dx++)
                            {
                                // Calculate slope
                                int slopeY = (int)(dx * slopeRatio);
                                
                                // Dig a wider section of the sloped tunnel
                                int digX = currentX + (goingRight ? dx : -dx);

                                // First pass: Place walls in a thick area around where the tunnel will be
                                for (int w = -wallThickness; w <= wallThickness; w++)
                                {
                                    for (int h = -wallThickness; h <= tunnelHeight + wallThickness; h++)
                                    {
                                        int wallX = digX + w;
                                        int wallY = currentY + slopeY + h;
                                        if (World.IsValidTile(wallX, wallY))
                                        {
                                            World.SetWall(wallX, wallY, EWallType.Dirt);
                                        }
                                    }
                                }

                                // Second pass: Carve out the actual tunnel
                                for (int w = -2; w <= 2; w++)
                                {
                                    for (int h = 0; h < tunnelHeight; h++)
                                    {
                                        int tunnelX = digX + w;
                                        int tunnelY = currentY + slopeY + h;
                                        if (World.IsValidTile(tunnelX, tunnelY))
                                        {
                                            World.SetTile(tunnelX, tunnelY, ETileType.Air);
                                        }
                                    }
                                }
                            }

                            // Update position for next segment
                            currentX += goingRight ? horizontalLength : -horizontalLength;
                            currentY += (int)(horizontalLength * slopeRatio);
                            goingRight = !goingRight;
                            segmentCount++;

                            // Add extra wall coverage at segment transitions
                            for (int w = -wallThickness; w <= wallThickness; w++)
                            {
                                for (int h = -wallThickness; h <= tunnelHeight + wallThickness; h++)
                                {
                                    int transitionX = currentX + w;
                                    int transitionY = currentY + h;
                                    if (World.IsValidTile(transitionX, transitionY))
                                    {
                                        World.SetWall(transitionX, transitionY, EWallType.Dirt);
                                        if (Math.Abs(w) >= 2 || h < 0 || h >= tunnelHeight)
                                        {
                                            World.SetTile(transitionX, transitionY, ETileType.Stone);
                                        }
                                    }
                                }
                            }

                            // Carve out the transition area
                            for (int w = -2; w <= 2; w++)
                            {
                                for (int h = 0; h < tunnelHeight; h++)
                                {
                                    int transitionX = currentX + w;
                                    int transitionY = currentY + h;
                                    if (World.IsValidTile(transitionX, transitionY))
                                    {
                                        World.SetTile(transitionX, transitionY, ETileType.Air);
                                    }
                                }
                            }
                        }

                        // Store the endpoint of this entrance tunnel
                        entranceEndPoints.Add((currentX, currentY));
                    }

                    // Third pass: Connect entrances (limited connections)
                    for (int i = 0; i < Math.Min(entranceEndPoints.Count, 10); i++) // Limit max connections
                    {
                        if (cts.Token.IsCancellationRequested) return;

                        var startPoint = entranceEndPoints[i];
                        
                        // Only connect to next point if close
                        if (i + 1 < entranceEndPoints.Count)
                        {
                            var endPoint = entranceEndPoints[i + 1];
                            if (Math.Abs(startPoint.x - endPoint.x) < 80) // Reduced distance
                            {
                                GenerateConnectingTunnel(startPoint.x, startPoint.y, endPoint.x, endPoint.y);
                            }
                        }
                        
                        // Reduced number of branches
                        int numBranches = new Random(wparams.seed + startPoint.x).Next(1, 3);
                        for (int b = 0; b < numBranches; b++)
                        {
                            if (cts.Token.IsCancellationRequested) return;

                            int branchX = startPoint.x + new Random(wparams.seed + startPoint.x + b).Next(-30, 30);
                            int branchY = startPoint.y + new Random(wparams.seed + startPoint.y + b).Next(-20, 20);
                            GenerateConnectingTunnel(startPoint.x, startPoint.y, branchX, branchY);
                        }
                    }

                    World.CompleteCurrent();
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // If we timeout, still mark as complete
                World.CompleteCurrent();
            }
        }
    }
}
