using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.Gameplay.Items;
using Tiled.ID;
using Tiled.UI;
using Tiled.UI.UserWidgets;

namespace Tiled
{
    public class World
    {
        public static int maxTilesX = -1;
        public static int maxTilesY = -1;

        public static bool renderWorld = false;
        public const int TILESIZE = 16;
        public int seed = 0;
        public static Rectangle invalidFrame = new Rectangle(-1, -1, -1, -1);
        public static ETileType[,] tiles;
        public static EWallType[,] walls;
        public static Rectangle[,] tileFramesCached;
        public static Rectangle[,] wallFramesCached;
        public static uint[,] lightMap;
        public float worldTime = 8.0f;
        public float timeSpeed = 0.0002f;
        public float timeSpeedMultiplier = 1.0f;
        public const float gravity = 0.43f;

        public Progress<WorldGenProgress> currentTaskProgress;
        private static TaskCompletionSource<bool> currentCompletionSource;
        public static List<WorldGenTask> tasks = new List<WorldGenTask>();
        public delegate void TaskProgressChanged(object sender, WorldGenProgress e);
        public event TaskProgressChanged taskProgressChanged;

        public static int[] surfaceHeights;
        public static int cavesLayerHeight = 0;
        public static int cavernsLayerHeight = 0;
        public static int averageSurfaceHeight = 0;
        public static sbyte[,] tileBreak;
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
            surfaceHeights = new int[maxTilesX];
            tileBreak = new sbyte[maxTilesX, maxTilesY];

            for (int i = 0; i < maxTilesX; i++)
            {
                for (int j = 0; j < maxTilesY; j++)
                {
                    tileFramesCached[i, j] = invalidFrame;
                    wallFramesCached[i, j] = invalidFrame;
                    lightMap[i, j] = 0;
                    tileBreak[i, j] = -128;
                }
            }
        }

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
                        maxTilesX = World.maxTilesX,
                        maxTilesY = World.maxTilesY,
                        seed = this.seed,
                    };

                    InitTasks();
                    renderWorld = false;

                    // Wait for the world generation to complete
                    await RunTasks(newParams);

                    if (LoadWorld(false) && !Main.isClient)
                    {
                        Program.GetGame().CreatePlayer(new Vector2(newParams.maxTilesX / 2, 0));
                        renderWorld = true;
                        Main.inTitle = false;
                    }
                    _genTaskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    _genTaskCompletionSource.SetException(ex);
                    throw;
                }
            });
        }


        int lightUpdateCounter = 0;
        public void UpdateWorld()
        {
            timeSpeedMultiplier = Main.inTitle ? 128.0f : 2.0f;

            //HOURS IN DAY
            const float h = 24.0f;

            //Day Length Exponent (Higher num = night shorter)
            const float dnExp = 2.0f;

            if(!Main.isClient)
            {
                worldTime = (worldTime + (timeSpeed * timeSpeedMultiplier)) % h;
            }
            else
            {
                //client doesnt predict time right now
                //worldTime += timeSpeed / Main.SERVER_TICKRATE;
            }
            
            Lighting.SKY_LIGHT_MULT = Math.Clamp((float)Math.Sin(Math.Pow(worldTime / h, dnExp) * (Math.PI / 0.5f)), 0.0f, 1.0f);
            lightUpdateCounter++;

            if(lightUpdateCounter >= 30 && renderWorld && !Lighting.isPerformingGlobalLightUpdate)
            {
                lightUpdateCounter = 0;
                Lighting.QueueGlobalLightUpdate();
            }

            //System.Diagnostics.Debug.WriteLine("world: " + worldTime + " && " + Lighting.SKY_LIGHT_MULT + " LIGHTVAL: " + Lighting.CalculateSkyLight(1));
        }

        public void InitTasks()
        {
            tasks.Add(new WGT_Terrain("Terrain"));
        }

        public async Task RunTasks(WorldGenParams newParams)
        {
            maxTilesX = newParams.maxTilesX;
            maxTilesY = newParams.maxTilesY;
            Init(); // RE INIT

            foreach (var task in tasks)
            {
                currentTaskProgress = new Progress<WorldGenProgress>();
                currentTaskProgress.ProgressChanged += CurrentTaskProgressChanged;

                // Important: We need to await the actual task execution
                var runTask = task.Run(currentTaskProgress, newParams);
                await runTask; // Wait for the actual task to complete

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

        public bool LoadWorld(bool fromFile, string? filePath = null)
        {
            if(fromFile)
            {
                if(filePath == null)
                {
                    UWMessage msg = HUD.CreateWidget<UWMessage>(Program.GetGame().localHUD, "ERROR \n LoadWorld(bool, string?) filePath was null! \n Game will close");
                    msg.SetGeometry(new Vector2(720, 480), AnchorPosition.Center);
                    return false;
                }
            }
            else
            {
                worldTime = 8.0f;
            }
            return true;
        }



        #region UTIL_CHECKS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(Array a, int i, int j)
        {
            if(a == null)
            {
                return false;
            }

            return !(i < 0 || j < 0 || i >= a.GetLength(0) || j >= a.GetLength(1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidTile(int x, int y)
        {
            if (!IsValidIndex(tiles, x, y))
                return false;

            return tiles[x, y] != ETileType.Air;
        }

        public static bool IsValidForTileFrame(int x, int y)
        {
            return IsValidTile(x, y) && TileID.GetTile(tiles[x, y]).useFrames && !TileID.GetTile(tiles[x, y]).hangingOnWalls;
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

        public static bool HasDirectNeighbors(int x, int y)
        {
            bool r = IsValidTile(x + 1, y) && !TileID.GetTile(tiles[x + 1, y]).hangingOnWalls;
            bool l = IsValidTile(x - 1, y) && !TileID.GetTile(tiles[x - 1, y]).hangingOnWalls;
            bool t = IsValidTile(x, y - 1) && !TileID.GetTile(tiles[x, y - 1]).hangingOnWalls;
            bool b = IsValidTile(x, y + 1) && !TileID.GetTile(tiles[x, y + 1]).hangingOnWalls;

            return (r || l || t || b);
        }

        public static bool IsValidForTilePlacement(int x, int y)
        {
            return tiles[x, y] == ETileType.Air && HasDirectNeighbors(x, y);
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

            bool r = IsValidForTileFrame(x + 1, y) && !tileData.ignoreNeighbors.R;
            bool l = IsValidForTileFrame(x - 1, y) && !tileData.ignoreNeighbors.L;
            bool t = IsValidForTileFrame(x, y - 1) && !tileData.ignoreNeighbors.T;
            bool b = IsValidForTileFrame(x, y + 1) && !tileData.ignoreNeighbors.B;

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

            bool r = IsValidForTileFrame(x + 1, y) && !tileData.ignoreNeighbors.R;
            bool l = IsValidForTileFrame(x - 1, y) && !tileData.ignoreNeighbors.L;
            bool b = IsValidForTileFrame(x, y + 1) && !tileData.ignoreNeighbors.B;

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

        public static void BreakTile(int x, int y, sbyte pickPower, sbyte axePower)
        {
            Tile tileData = TileID.GetTile(tiles[x, y]);
            if (pickPower > tileData.minPick || axePower > tileData.minAxe)
            {
                if (tileBreak[x, y] == -128)
                {
                    tileBreak[x, y] = (sbyte)(TileID.GetTile(tiles[x, y]).hardness - (pickPower + axePower));
                    goto c;
                }

                tileBreak[x, y] = (sbyte)(tileBreak[x, y] - (pickPower + axePower));

                c:
                if (tileBreak[x, y] < 0)
                {
                    DestroyTile(x, y);
                    tileBreak[x, y] = -128;
                }
            }
            
        }

        public static void DestroyTile(int x, int y)
        {
            Tile t = TileID.GetTile(tiles[x, y]);

            

            SetTile(x, y, ETileType.Air);

            UpdateTile(x, y);
            UpdateTile(x + 1, y);
            UpdateTile(x - 1, y);
            UpdateTile(x, y - 1);
            UpdateTile(x, y + 1);

            if (t.itemDrop == EItemType.None)
            {
                return;
            }
            
            var item = EItem.CreateItem(t.itemDrop);
            item.velocity.Y = -5.0f;
            item.position = new Vector2(x * TILESIZE, y * TILESIZE);
        }

        public static void UpdateTile(int x, int y)
        {
            if (tiles[x, y] == ETileType.Torch)
            {
                if (!HasDirectNeighbors(x, y))
                {
                    DestroyTile(x, y);
                }
            }
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