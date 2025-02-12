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

            switch(type)
            {
                case ETileType.None:
                    t.render = false;
                    break;

                case ETileType.Dirt:
                    t.render = true;
                    t.sprite = Program.GetGame().Content.Load<Texture2D>("Tiles/debugTile");
                    break;
            }

            return t;
        }
    }
}
