using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.Gameplay.Items;
using Tiled.ID;
using Tiled.Networking.Shared;
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
        public float worldTime = 11.0f;
        public float timeSpeed = 0.0002f;
        public float timeSpeedMultiplier = 1.0f;
        public const float gravity = 0.43f;

        public Progress<WorldGenProgress> currentTaskProgress;
        private static TaskCompletionSource<bool> currentCompletionSource;
        public static List<WorldGenTask> tasks = new List<WorldGenTask>();
        public delegate void TaskProgressChanged(object sender, WorldGenProgress e);
        public event TaskProgressChanged taskProgressChanged;
        public delegate void WorldGenFinished();
        public event WorldGenFinished worldGenFinished;

        public static int[] surfaceHeights;
        public static int cavesLayerHeight = 0;
        public static int cavernsLayerHeight = 0;
        public static int averageSurfaceHeight = 0;
        public static sbyte[,] tileBreak;
        public static bool isGenerating = false;

        public static float difficulty = 1.0f;

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
            //tasks = new List<WorldGenTask>();

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
            isGenerating = true;
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
                        maxTilesX = maxTilesX,
                        maxTilesY = maxTilesY,
                        seed = seed,
                    };

                    InitTasks();
                    //renderWorld = false;

                    // Wait for the world generation to complete
                    await RunTasks(newParams);

                    worldGenFinished?.Invoke();

                    ClearTasks();

                    if (LoadWorld(false) && Main.netMode == ENetMode.Standalone)
                    {
                        Program.GetGame().CreatePlayer(new Vector2(newParams.maxTilesX / 2, 0));
                        renderWorld = true;
                        Main.inTitle = false;
                    }

                    isGenerating = false;
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

            if(Main.netMode != ENetMode.Client)
            {
                worldTime = (worldTime + (timeSpeed * timeSpeedMultiplier)) % h;
            }
            else
            {
                //client doesnt predict time right now
                //worldTime += timeSpeed / Main.SERVER_TICKRATE;
            }
            
            Lighting.skyLightMultiplier = Math.Clamp((float)Math.Sin(Math.Pow(worldTime / h, dnExp) * (Math.PI / 0.5f)), 0.0f, 1.0f);
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
            tasks = new List<WorldGenTask>();
            tasks.Clear();
            tasks.Add(new WGT_Terrain("Terrain"));
            tasks.Add(new WGT_Caves("Caves"));
            tasks.Add(new WGT_PlaceTrees("Trees"));
        }

        public void ClearTasks()
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i] = null;
            }
            tasks.Clear();
            tasks = null;
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
                Debug.WriteLine(task.taskName);
            }

            return;
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
            return (tiles[x, y] == ETileType.Air) && HasDirectNeighbors(x, y) && (Collision.CollisionStatics.isEntityWithinRect(new((x * TILESIZE), (y * TILESIZE), TILESIZE, TILESIZE)) == null);
        }

        public static bool IsValidForTilePlacement(int x, int y, ETileType type)
        {
            if(!TileID.GetTile(type).collision)
            {
                return tiles[x, y] == ETileType.Air && HasDirectNeighbors(x, y);
            }

            return ((tiles[x, y] == ETileType.Air && HasDirectNeighbors(x, y)) || IsValidWall(x, y)) && (Collision.CollisionStatics.isEntityWithinRect(new((x * TILESIZE), (y * TILESIZE), TILESIZE, TILESIZE)) == null);
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

            bool r = IsValidForTileFrame(x + 1, y) && !tileData.ignoreNeighbors.R && IsPartOfAllowedTileFrameTypes(x + 1, y, tileData) && (tiles[x + 1, y] != ETileType.TreeTrunk || tiles[x, y] == ETileType.TreeTrunk);
            bool l = IsValidForTileFrame(x - 1, y) && !tileData.ignoreNeighbors.L && IsPartOfAllowedTileFrameTypes(x - 1, y, tileData) && (tiles[x - 1, y] != ETileType.TreeTrunk || tiles[x, y] == ETileType.TreeTrunk);
            bool t = IsValidForTileFrame(x, y - 1) && !tileData.ignoreNeighbors.T && IsPartOfAllowedTileFrameTypes(x, y - 1, tileData) && (tiles[x, y - 1] != ETileType.TreeTrunk || tiles[x, y] == ETileType.TreeTrunk);
            bool b = IsValidForTileFrame(x, y + 1) && !tileData.ignoreNeighbors.B && IsPartOfAllowedTileFrameTypes(x, y + 1, tileData) && (tiles[x, y + 1] != ETileType.TreeTrunk || tiles[x, y] == ETileType.TreeTrunk);

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

        public static bool IsPartOfAllowedTileFrameTypes(int x, int y, Tile tileData)
        {
            if(tileData.useSpecificTileTypesForFrame)
            {
                return tileData.frameOnlyTypes[(int)tiles[x, y]] != false;
            }
            return true;
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

        public static void SetTile(int x, int y, ETileType type, bool noBroadcast = true)
        {
            if(!IsValidIndex(tiles, x, y))
            { 
                return;
            }

            tiles[x, y] = type;
            UpdateTileFramesAt(x, y);
            Lighting.QueueLightUpdate(x, y);
#if !TILEDSERVER
        /*
            if(!noBroadcast && Main.netMode == ENetMode.Client)
            {
                Main.netClient.SendTileSquare(x, y, type);
            }
        */
#else
            if(!noBroadcast)
            {
                TileChangePacket t = new TileChangePacket();
                t.x = x;
                t.y = y;
                t.tileType = type;
                Main.netServer.SendTileSquare(t);
            }
#endif
        }

        public static void BreakTile(int x, int y, sbyte pickPower, sbyte axePower)
        {
            if(!IsValidTile(x, y))
            {
                return;
            }

            if(!IsValidForBreaking(x, y))
            {
                return;
            }

            Tile tileData = TileID.GetTile(tiles[x, y]);

            if ((tileData.minPick != -1 && pickPower > tileData.minPick) || (tileData.minAxe != -1 && axePower > tileData.minAxe))
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

                //Debug.WriteLine("DESTROY TILE: " + "x: " + x + " y: " + y + " breakLevel: " + tileBreak[x, y]);
            }
            
        }

        public static bool IsValidForBreaking(int x, int y)
        {
            if(tiles[x, y] != ETileType.TreeTrunk)
            {
                return tiles[x, y - 1] != ETileType.TreeTrunk;
            }
            return true;
        }

        public static void CreateExplosion(int centerX, int centerY, int radius, sbyte maxPickPower, sbyte maxAxePower)
        {
            int minX = Math.Max(0, centerX - radius);
            int maxX = Math.Min(tiles.GetLength(0) - 1, centerX + radius);
            int minY = Math.Max(0, centerY - radius);
            int maxY = Math.Min(tiles.GetLength(1) - 1, centerY + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));

                    if (distance <= radius)
                    {
                        double powerMultiplier = 1.0 - (distance / radius);

                        sbyte pickPower = (sbyte)(maxPickPower * powerMultiplier);
                        sbyte axePower = (sbyte)(maxAxePower * powerMultiplier);

                        BreakTile(x, y, pickPower, axePower);
                    }
                }
            }
        }

        public static void DestroyTile(int x, int y)
        {
            Tile t = TileID.GetTile(tiles[x, y]);

            if(!t.destroyedByExplosion)
            {
                return;
            }

#if TILEDSERVER
            if(Main.netMode == ENetMode.Server)
            {
                TileChangePacket newTile = new TileChangePacket();
                newTile.x = x;
                newTile.y = y;
                newTile.tileType = ETileType.Air;

                Main.netServer.SendTileSquare(newTile);

            }
#endif

            if (Main.netMode != ENetMode.Server)
            {
                SetTile(x, y, ETileType.Air, false);
            }

            UpdateTile(x, y);
            UpdateTile(x + 1, y);
            UpdateTile(x - 1, y);
            UpdateTile(x, y - 1);
            UpdateTile(x, y + 1);

            if (t.itemDrop == EItemType.None)
            {
                return;
            }

#if TILEDSERVER
            Random r = new Random((int)((Main.runtime + x + y - 12.0f) / 131.0f));
            
            Main.netServer.ServerSpawnEntity(ENetEntitySpawnType.Item, (byte)0, t.itemDrop, (byte)0, new(x * TILESIZE, y * TILESIZE), new((2.0f * r.NextSingle() - 1.0f) * 5.0f, -5.0f));
            return;
#else
            if(Main.netMode == ENetMode.Standalone)
            {
                EItem newItem = EItem.CreateItem(t.itemDrop);
                newItem.position = new(x * TILESIZE, y * TILESIZE);
                newItem.velocity = new(0.0f, -5.0f);
            }
#endif
        }

        public static void UpdateTile(int x, int y)
        {
            if(!IsValidIndex(tiles, x, y))
            {
                return;
            }

            if (tiles[x, y] == ETileType.Torch)
            {
                if (!HasDirectNeighbors(x, y))
                {
                    DestroyTile(x, y);
                }
            }

            if (tiles[x, y] == ETileType.TreeTrunk)
            {
                if (tiles[x, y + 1] == ETileType.Air)
                {
                    DestroyTile(x, y);
                }
                else
                {
                    if(tiles[x, y + 1] != ETileType.TreeTrunk)
                    {
                        if (tiles[x + 1, y] == ETileType.Air && tiles[x - 1, y] == ETileType.Air)
                        {
                            DestroyTile(x, y);
                        }
                    }
                }
            }

            if (tiles[x, y] == ETileType.TreeLeaves)
            {
                if (tiles[x, y + 1] == ETileType.Air)
                {
                    DestroyTile(x, y);
                }
                else
                {
                    if (tiles[x, y + 1] != ETileType.TreeLeaves)
                    {
                        if (tiles[x + 1, y] == ETileType.Air && tiles[x - 1, y] == ETileType.Air)
                        {
                            DestroyTile(x, y);
                        }
                    }
                }
            }
        }

        public static (ETileType, ETileType, ETileType, ETileType) GetNeighborTypes(int x, int y)
        {
            (ETileType, ETileType, ETileType, ETileType) tuple;

            tuple.Item1 = tiles[x + 1, y];
            tuple.Item2 = tiles[x - 1, y];
            tuple.Item3 = tiles[x, y + 1];
            tuple.Item4 = tiles[x, y - 1];

            return tuple;
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

        public static void DigTunnel(int startX, int startY, int length, float curviness = 0.2f, int width = 3)
        {
            Random random = new Random();

            double angle = random.NextDouble() * Math.PI * 2;

            float currentX = startX;
            float currentY = startY;

            for (int step = 0; step < length; step++)
            {
                angle += (random.NextDouble() - 0.5) * curviness;

                currentX += (float)Math.Cos(angle);
                currentY += (float)Math.Sin(angle);

                int tileX = (int)Math.Round(currentX);
                int tileY = (int)Math.Round(currentY);

                for (int dy = -width; dy <= width; dy++)
                {
                    for (int dx = -width; dx <= width; dx++)
                    {
                        int digX = tileX + dx;
                        int digY = tileY + dy;

                        if (digX >= 0 && digX < World.tiles.GetLength(0) &&
                            digY >= 0 && digY < World.tiles.GetLength(1))
                        {
                            double distance = Math.Sqrt(dx * dx + dy * dy);

                            if (distance <= width)
                            {
                                if (distance <= width - 1 || random.NextDouble() > 0.4)
                                {
                                    // Only place walls if we're digging through solid blocks below the surface
                                    if (digY > World.surfaceHeights[digX] && digY < World.averageSurfaceHeight + 80 && World.tiles[digX, digY] != ETileType.Air)
                                    {
                                        SetWall(digX, digY, EWallType.Dirt);
                                        SetWall(digX + 1, digY, EWallType.Dirt);
                                        SetWall(digX - 1, digY, EWallType.Dirt);
                                        SetWall(digX, digY - 1, EWallType.Dirt);
                                        SetWall(digX, digY + 1, EWallType.Dirt);
                                    }
                                    
                                    SetTile(digX, digY, ETileType.Air);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void GenerateCaveSystem(int startX, int startY, int numTunnels = 8, int maxLength = 60, int minWidth = 3, int maxWidth = 7)
        {
            Random random = new Random(Program.GetGame().world.seed);

            List<(int x, int y)> junctionPoints = new List<(int x, int y)> { (startX, startY) };

            for (int i = 0; i < numTunnels; i++)
            {
                int junctionIndex = random.Next(junctionPoints.Count);
                int junctionX = junctionPoints[junctionIndex].x;
                int junctionY = junctionPoints[junctionIndex].y;

                int tunnelLength = random.Next(1, Math.Max(maxLength, 1)); // Increased minimum length
                float curviness = 0.15f + (float)random.NextDouble() * 0.4f; // Increased variation in curves
                int width = random.Next(minWidth, maxWidth);

                // Dig the tunnel
                DigTunnel(junctionX, junctionY, tunnelLength, curviness, width);

                float angle = (float)(random.NextDouble() * Math.PI * 2);
                int endX = junctionX + (int)(Math.Cos(angle) * tunnelLength);
                int endY = junctionY + (int)(Math.Sin(angle) * tunnelLength);

                endX = Math.Clamp(endX, 0, World.tiles.GetLength(0) - 1);
                endY = Math.Clamp(endY, 0, World.tiles.GetLength(1) - 1);

                junctionPoints.Add((endX, endY));
            }

            foreach ((int x, int y) in junctionPoints)
            {
                if (random.NextDouble() > 0.3) // Increased chance of chambers
                {
                    int chamberRadius = random.Next(6, 12); // Larger chambers
                    DigChamber(x, y, chamberRadius);
                }
            }
        }

        public static void DigChamber(int centerX, int centerY, int radius)
        {
            Random random = new Random();

            int minX = Math.Max(0, centerX - radius);
            int maxX = Math.Min(World.tiles.GetLength(0) - 1, centerX + radius);
            int minY = Math.Max(0, centerY - radius);
            int maxY = Math.Min(World.tiles.GetLength(1) - 1, centerY + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));

                    double noiseThreshold = radius * (0.8 + random.NextDouble() * 0.3);

                    if (distance <= noiseThreshold)
                    {
                        SetTile(x, y, ETileType.Air);

                        /*SetWall(x, y, EWallType.Dirt);
                        SetWall(x + 1, y, EWallType.Dirt);
                        SetWall(x - 1, y, EWallType.Dirt);
                        SetWall(x, y - 1, EWallType.Dirt);
                        SetWall(x, y + 1, EWallType.Dirt);*/
                    }
                }
            }
        }
    }
}