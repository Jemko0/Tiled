using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Tiled.DataStructures;

namespace Tiled
{
    
    public class World
    {
        public static int maxTilesX = 256;
        public static int maxTilesY = 256;

        bool renderWorld = true;
        public const int TILESIZE = 16;
        public static ETileType[,] tiles;
        public static EWallType[,] walls;
        public static Rectangle[,] tileFramesCached;
        public static Rectangle[,] wallFramesCached;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public int DrawOrder => 0;

        public bool Visible => renderWorld;

        public World()
        {
        }

        public void Init()
        {
            tiles = new ETileType[maxTilesX, maxTilesY];
            walls = new EWallType[maxTilesX, maxTilesY];
            tileFramesCached = new Rectangle[maxTilesX, maxTilesY];
            wallFramesCached = new Rectangle[maxTilesX, maxTilesY];
            walls.Initialize();
            tiles.Initialize();

            for (int i = 0; i < maxTilesX; i++)
            {
                for (int j = 0; j < maxTilesY; j++)
                {
                    tileFramesCached[i, j] = new Rectangle(-1, -1, -1, -1);
                    wallFramesCached[i, j] = new Rectangle(-1, -1, -1, -1);
                    tiles[i, j] = ETileType.Dirt;
                    walls[i, j] = EWallType.Dirt;
                }
            }
        }

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
            return frame != new Rectangle(-1, -1, -1, -1);
        }


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
    
    
    }
}
