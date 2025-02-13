using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Tiled.DataStructures;

namespace Tiled
{
    
    public class World
    {
        public static int maxTilesX = 256;
        public static int maxTilesY = 256;

        bool renderWorld = true;
        public const int renderTileSize = 16;
        public static ETileType[,] tiles;
        public static EWallType[,] walls;
        
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
            tiles.Initialize();
            for (int i = 0; i < maxTilesX; i++)
            {
                for (int j = 0; j < maxTilesY; j++)
                {
                    tiles[i, j] = ETileType.Dirt;
                    walls[i, j] = EWallType.Dirt;
                }
            }
        }
    }
}
