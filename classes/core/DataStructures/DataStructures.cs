using Microsoft.Xna.Framework.Graphics;

namespace Tiled.DataStructures
{
    public struct Tile
    {
        public bool render;
        public Texture2D sprite;
    }

    public enum ETileType
    {
        None = 0,
        Dirt,
    }
}
