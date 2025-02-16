﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tiled.DataStructures;

namespace Tiled
{
    public class World
    {
        public static int maxTilesX = -1;
        public static int maxTilesY = -1;

        public static bool renderWorld = false;
        public const int TILESIZE = 16;
        public static Rectangle invalidFrame = new Rectangle(-1, -1, -1, -1);
        public static ETileType[,] tiles;
        public static EWallType[,] walls;
        public static Rectangle[,] tileFramesCached;
        public static Rectangle[,] wallFramesCached;
        public static uint[,] lightMap;
        public float worldTime = 8.0f;
        public const float timeSpeed = 0.005f;

        public Progress<WorldGenProgress> currentTaskProgress;
        private static TaskCompletionSource<bool> currentCompletionSource;
        public static List<WorldGenTask> tasks = new List<WorldGenTask>();
        public delegate void TaskProgressChanged(object sender, WorldGenProgress e);
        public event TaskProgressChanged taskProgressChanged;
        
        public World()
        {
        }
        
        public void Init()
        {
            tiles = new ETileType[maxTilesX, maxTilesY];
            walls = new EWallType[maxTilesX, maxTilesY];
            tileFramesCached = new Rectangle[maxTilesX, maxTilesY];
            wallFramesCached = new Rectangle[maxTilesX, maxTilesY];
            lightMap = new uint[maxTilesX, maxTilesY];

            for (int i = 0; i < maxTilesX; i++)
            {
                for (int j = 0; j < maxTilesY; j++)
                {
                    tileFramesCached[i, j] = invalidFrame;
                    wallFramesCached[i, j] = invalidFrame;
                    lightMap[i, j] = 0;
                }
            }
        }

        int lightUpdateCounter = 0;
        public void UpdateWorld()
        {
            //HOURS IN DAY
            const float h = 24.0f;

            //Day Length Exponent (Higher num = night shorter)
            const float dnExp = 3.0f;

            worldTime = (worldTime + timeSpeed) % h;
            
            Lighting.SKY_LIGHT_MULT = Math.Clamp((float)Math.Sin(Math.Pow(worldTime / h, dnExp) * (Math.PI / 0.5f)), 0.0f, 1.0f);
            lightUpdateCounter++;

            if(lightUpdateCounter >= 30 && renderWorld)
            {
                lightUpdateCounter = 0;
                Lighting.QueueGlobalLightUpdate();
            }

            //System.Diagnostics.Debug.WriteLine("world: " + worldTime + " && " + Lighting.SKY_LIGHT_MULT + " LIGHTVAL: " + Lighting.CalculateSkyLight(1));
        }

        public void InitTasks()
        {
            tasks.Add(new WGT_TestFillWorld("task"));
        }

        public async Task RunTasks(WorldGenParams newParams)
        {
            maxTilesX = newParams.maxTilesX;
            maxTilesY = newParams.maxTilesY;
            Init(); //RE INIT

            foreach (var task in tasks)
            {
                currentCompletionSource = new TaskCompletionSource<bool>();
                currentTaskProgress = new Progress<WorldGenProgress>();
                currentTaskProgress.ProgressChanged += CurrentTaskProgressChanged;

                var runTask = task.Run(currentTaskProgress, newParams);
                await Task.WhenAll(runTask, currentCompletionSource.Task);
            }
        }

        public static void CompleteCurrent()
        {
            if (currentCompletionSource != null && !currentCompletionSource.Task.IsCompleted)
            {
                currentCompletionSource.SetResult(true);
            }
        }

        private void CurrentTaskProgressChanged(object sender, WorldGenProgress e)
        {
            Debug.WriteLine("TASK: " + e.PercentComplete);
            taskProgressChanged?.Invoke(this, e);
        }



        #region UTIL_CHECKS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(Array a, int i, int j)
        {
            return !(i < 0 || j < 0 || i >= a.GetLength(0) || j >= a.GetLength(1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidTile(int x, int y)
        {
            if (!IsValidIndex(tiles, x, y))
                return false;

            return tiles[x, y] != ETileType.Air;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidWall(int x, int y)
        {
            if (!IsValidIndex(tiles, x, y))
                return false;

            return walls[x, y] != EWallType.Air;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidTileOrWall(int x, int y)
        {
            return IsValidTile(x, y) || IsValidWall(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidFrame(Rectangle frame)
        {
            return frame != invalidFrame;
        }
        #endregion

        public static Rectangle GetTileFrame(int x, int y, Tile tileData)
        {
            if (!tileData.useFrames)
            {
                return new Rectangle(0, 0, TILESIZE, TILESIZE);
            }

            if (tileData.hangingOnWalls)
            {
                tileFramesCached[x, y] = GetHangingTileFrame(x, y, tileData);
                return tileFramesCached[x, y];
            }

            int frameX = 0;
            int frameY = 0;
            int frameSlot = tileData.frameSize + tileData.framePadding;

            

            if (IsValidFrame(tileFramesCached[x, y]))
            {
                return tileFramesCached[x, y];
            }

            bool r = IsValidTile(x + 1, y) && !tileData.ignoreNeighbors.R;
            bool l = IsValidTile(x - 1, y) && !tileData.ignoreNeighbors.L;
            bool t = IsValidTile(x, y - 1) && !tileData.ignoreNeighbors.T;
            bool b = IsValidTile(x, y + 1) && !tileData.ignoreNeighbors.B;

            var tuple = (r, l, t, b);

            switch (tuple)
            {

                case (true, false, false, false): //R
                    frameX = frameSlot * 1;
                    frameY = frameSlot * 0;
                    break;

                case (false, true, false, false): //L
                    frameX = frameSlot * 2;
                    frameY = frameSlot * 0;
                    break;

                case (false, false, true, false): //T
                    frameX = frameSlot * 3;
                    frameY = frameSlot * 0;
                    break;

                case (false, false, false, true): //B
                    frameX = frameSlot * 4;
                    frameY = frameSlot * 0;
                    break;

                case (true, true, true, true): //ALL
                    frameX = frameSlot * 5;
                    frameY = frameSlot * 0;
                    break;

                case (true, true, false, false): //RL && LR
                    frameX = frameSlot * 0;
                    frameY = frameSlot * 1;
                    break;

                case (true, false, true, false): //RT
                    frameX = frameSlot * 1;
                    frameY = frameSlot * 1;
                    break;

                case (true, false, false, true): //RB
                    frameX = frameSlot * 2;
                    frameY = frameSlot * 1;
                    break;

                case (false, true, true, false): //LT
                    frameX = frameSlot * 3;
                    frameY = frameSlot * 1;
                    break;

                case (false, true, false, true): //LB
                    frameX = frameSlot * 4;
                    frameY = frameSlot * 1;
                    break;

                case (false, false, true, true): //TB
                    frameX = frameSlot * 5;
                    frameY = frameSlot * 1;
                    break;

                case (true, false, true, true): //RTB
                    frameX = frameSlot * 0;
                    frameY = frameSlot * 2;
                    break;

                case (false, true, true, true): //LTB
                    frameX = frameSlot * 1;
                    frameY = frameSlot * 2;
                    break;

                case (true, true, false, true): //RLB
                    frameX = frameSlot * 2;
                    frameY = frameSlot * 2;
                    break;

                case (true, true, true, false): //RLT
                    frameX = frameSlot * 3;
                    frameY = frameSlot * 2;
                    break;
            }

            tileFramesCached[x, y] = new Rectangle(frameX, frameY, TILESIZE, TILESIZE);
            return tileFramesCached[x, y];
        }

        public static Rectangle GetHangingTileFrame(int x, int y, Tile tileData)
        {
            int frameX = 0;
            int frameY = 0;
            int frameSlot = tileData.frameSize + tileData.framePadding;

            bool r = IsValidTile(x + 1, y) && !tileData.ignoreNeighbors.R;
            bool l = IsValidTile(x - 1, y) && !tileData.ignoreNeighbors.L;
            bool b = IsValidTile(x, y + 1) && !tileData.ignoreNeighbors.B;

            if(b)
            {
                frameX = 0;
                frameY = 0;
            }

            if (r)
            {
                frameX = frameSlot ;
                frameY = 0;
            }

            if(l)
            {
                frameX = frameSlot * 2;
                frameY = 0;
            }

            return new Rectangle(frameX, frameY, tileData.frameSize, tileData.frameSize);
        }
        public static Rectangle GetWallFrame(int x, int y, Wall wallData)
        {
            int frameX = 0;
            int frameY = 0;
            int frameSlot = wallData.frameSize + wallData.framePadding;

            if (!wallData.useFrames)
            {
                return new Rectangle(0, 0, TILESIZE, TILESIZE);
            }

            if (IsValidFrame(wallFramesCached[x, y]))
            {
                return wallFramesCached[x, y];
            }

            bool r = IsValidWall(x + 1, y);
            bool l = IsValidWall(x - 1, y);
            bool t = IsValidWall(x, y - 1);
            bool b = IsValidWall(x, y + 1);

            var tuple = (r, l, t, b);

            switch (tuple)
            {

                case (true, false, false, false): //R
                    frameX = frameSlot * 1;
                    frameY = frameSlot * 0;
                    break;

                case (false, true, false, false): //L
                    frameX = frameSlot * 2;
                    frameY = frameSlot * 0;
                    break;

                case (false, false, true, false): //T
                    frameX = frameSlot * 3;
                    frameY = frameSlot * 0;
                    break;

                case (false, false, false, true): //B
                    frameX = frameSlot * 4;
                    frameY = frameSlot * 0;
                    break;

                case (true, true, true, true): //ALL
                    frameX = frameSlot * 5;
                    frameY = frameSlot * 0;
                    break;

                case (true, true, false, false): //RL && LR
                    frameX = frameSlot * 0;
                    frameY = frameSlot * 1;
                    break;

                case (true, false, true, false): //RT
                    frameX = frameSlot * 1;
                    frameY = frameSlot * 1;
                    break;

                case (true, false, false, true): //RB
                    frameX = frameSlot * 2;
                    frameY = frameSlot * 1;
                    break;

                case (false, true, true, false): //LT
                    frameX = frameSlot * 3;
                    frameY = frameSlot * 1;
                    break;

                case (false, true, false, true): //LB
                    frameX = frameSlot * 4;
                    frameY = frameSlot * 1;
                    break;

                case (false, false, true, true): //TB
                    frameX = frameSlot * 5;
                    frameY = frameSlot * 1;
                    break;

                case (true, false, true, true): //RTB
                    frameX = frameSlot * 0;
                    frameY = frameSlot * 2;
                    break;

                case (false, true, true, true): //LTB
                    frameX = frameSlot * 1;
                    frameY = frameSlot * 2;
                    break;

                case (true, true, false, true): //RLB
                    frameX = frameSlot * 2;
                    frameY = frameSlot * 2;
                    break;

                case (true, true, true, false): //RLT
                    frameX = frameSlot * 3;
                    frameY = frameSlot * 2;
                    break;
            }

            wallFramesCached[x, y] = new Rectangle(frameX, frameY, TILESIZE, TILESIZE);
            return wallFramesCached[x, y];
        }
    
        public static void UpdateTileFramesAt(int x, int y)
        {
            //ClearTileFrame(x, y);

            ClearTileFrame(x + 1, y);
            ClearTileFrame(x - 1, y);

            ClearTileFrame(x, y + 1);
            ClearTileFrame(x, y - 1);
        }

        public static void UpdateWallFramesAt(int x, int y)
        {
            //ClearTileFrame(x, y);

            ClearWallFrame(x + 1, y);
            ClearWallFrame(x - 1, y);

            ClearWallFrame(x, y + 1);
            ClearWallFrame(x, y - 1);
        }

        public static void SetTile(int x, int y, ETileType type)
        {
            if(!IsValidIndex(tiles, x, y))
            { 
                return;
            }

            tiles[x, y] = type;
            UpdateTileFramesAt(x, y);
            Lighting.QueueLightUpdate(x, y);
        }

        public static void SetWall(int x, int y, EWallType type)
        {
            if (!IsValidIndex(walls, x, y))
            {
                return;
            }

            walls[x, y] = type;
            UpdateWallFramesAt(x, y);
            Lighting.QueueLightUpdate(x, y);
        }

        public static void ClearTileFrame(int x, int y)
        {
            if(!IsValidIndex(tileFramesCached, x, y))
            {
                return;
            }

            tileFramesCached[x, y] = invalidFrame;
        }

        public static void ClearWallFrame(int x, int y)
        {
            if (!IsValidIndex(wallFramesCached, x, y))
            {
                return;
            }

            wallFramesCached[x, y] = invalidFrame;
        }
    }
}