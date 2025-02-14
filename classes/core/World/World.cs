using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Tiled.DataStructures;

namespace Tiled
{
    public class World
    {
        public static int maxTilesX = 256;
        public static int maxTilesY = 256;

        public static bool renderWorld = true;
        public const int TILESIZE = 16;
        public static Rectangle invalidFrame = new Rectangle(-1, -1, -1, -1);
        public static ETileType[,] tiles;
        public static EWallType[,] walls;
        public static Rectangle[,] tileFramesCached;
        public static Rectangle[,] wallFramesCached;
        public static uint[,] lightMap;
        public float worldTime = 23.0f;
        public const float timeSpeed = 0.02f;

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

                    if(j < maxTilesY / 2)
                    {
                        tiles[i, j] = ETileType.Air;
                        walls[i, j] = EWallType.Air;
                    }
                    else
                    {
                        tiles[i, j] = ETileType.Dirt;
                        walls[i, j] = EWallType.Dirt;
                    }
                    
                    lightMap[i, j] = 0;
                }
            }
        }

        int lightUpdateCounter = 0;
        public void UpdateWorld()
        {
            worldTime = (worldTime + timeSpeed) % 24.0f;
            Lighting.SKY_LIGHT_MULT = MathHelper.LerpPrecise(0.0f, 1.0f, Math.Abs(12.0f - worldTime) / 12.0f);
            lightUpdateCounter++;

            if(lightUpdateCounter >= 180)
            {
                lightUpdateCounter = 0;
                Lighting.QueueGlobalLightUpdate();
            }
            
            System.Diagnostics.Debug.WriteLine("world: " + worldTime + " && " + Lighting.SKY_LIGHT_MULT);
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
            int frameX = 0;
            int frameY = 0;
            int frameSlot = tileData.frameSize + tileData.framePadding;

            if(!tileData.useFrames)
            {
                return new Rectangle(0, 0, TILESIZE, TILESIZE);
            }

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