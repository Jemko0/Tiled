using Microsoft.Xna.Framework.Graphics;

namespace Tiled.DataStructures
{
    public struct Tile
    {
        public bool render;
        public Texture2D sprite;
    }

    public struct Wall
    {
        public bool render;
        public Texture2D sprite;
    }

    public enum ETileType
    {
        Air = 0,
        Dirt,
    }

    public enum EWallType
    {
        Air = 0,
        Dirt,
    }
}
