﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        int rockHeight = surfaceTileY + 80;
                        World.surfaceHeights[x] = surfaceTileY;

                        for (int y = surfaceTileY; y < wparams.maxTilesY; y++)
                        {
                            ETileType placeType = ETileType.Air;
                            if(y == surfaceTileY)
                            {
                                placeType = ETileType.Grass;
                            }
                            else
                            {
                                if(y > rockHeight)
                                {
                                    placeType = ETileType.Stone;
                                }
                                else
                                {
                                    placeType = ETileType.Dirt;
                                }
                            }
                            //World.walls[x, y] = EWallType.Dirt;
                            World.tiles[x, y] = placeType;
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

        private int CalcSurface(int x, WorldGenParams wparams, FastNoiseLite noise, int baseSurface)
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
            noise.SetFrequency(0.0075f);
            float pv = (noise.GetNoise(x, 0)) * 15.0f;

            float finalVal = (noise1 - noise2) - pv;
            return baseSurface + (int)finalVal;
        }
    }
}
