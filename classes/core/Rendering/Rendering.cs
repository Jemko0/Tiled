using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled
{
    public class Rendering
    {
        public static Rectangle GetTileTransform(int x, int y)
        {
            return new Rectangle((int)((x * World.renderTileSize) - Program.GetGame().localCamera.position.X), (int)((y * World.renderTileSize) - Program.GetGame().localCamera.position.Y), World.renderTileSize, World.renderTileSize);
        }
    }
}
