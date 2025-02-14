using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;

namespace Tiled.ID
{
    public class TileID
    {
        public static Tile GetTile(ETileType type)
        {
            Tile t = new();
            t.render = true;
            t.sprite = null;
            t.frameSize = 16;
            t.useFrames = true;
            t.framePadding = 2;
            t.ignoreNeighbors = new TileNeighbors(0, 0, 0, 0);
            t.light = 0;
            t.blockLight = 2;

            switch(type)
            {
                case ETileType.Air:
                    t.render = false;
                    t.blockLight = 1;
                    break;

                case ETileType.Dirt:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/dirt");
                    break;

                case ETileType.Torch:
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/torch");
                    t.ignoreNeighbors = new TileNeighbors(0, 0, 1, 1);
                    t.blockLight = 0;
                    t.light = 32;
                    break;
            }

            return t;
        }
    }

    public class WallID
    {
        public static Wall GetWall(EWallType type)
        {
            Wall w = new();
            w.render = true;
            w.sprite = null;
            w.useFrames = true;
            w.framePadding = 2;
            w.frameSize = 16;

            switch(type)
            {
                case EWallType.Air:
                    w.render = false;
                    break;

                case EWallType.Dirt:
                    w.render = true;
                    w.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/dirt");
                    break;
            }

            return w;
        }
    }
}
