using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;

namespace Tiled
{
    public class World
    {
        int maxTilesX = 256;
        int maxTilesY = 256;
        bool renderWorld = true;
        public const int tileSize = 16;
        public static ETileType[,] tiles;
        Texture2D empty = new Texture2D(Main.GetGame()._graphics.GraphicsDevice, 1, 1);

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public int DrawOrder => 0;

        public bool Visible => renderWorld;

        public World()
        {
            tiles = new ETileType[maxTilesX, maxTilesY];
            tiles.Initialize();
            empty.SetData(new Color[] { Color.HotPink });
            for (int i = 0; i < maxTilesX; i++)
            {
                for (int j = 0; j < maxTilesY; j++)
                {
                    tiles[i, j] = ETileType.Dirt;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch s, GraphicsDevice g)
        {
            for (int i = 0; i < maxTilesX; i++)
            {
                for (int j = 0; j < maxTilesY; j++)
                {
                    if (tiles[i, j] == ETileType.None)
                    {
                        return;
                    }

                    
                    
                    Rectangle drawRect = new(i, j, tileSize, tileSize);
                    s.Draw(empty, drawRect, Color.White);
                }
            }
        }
    }
}
