using Microsoft.Xna.Framework;

namespace Tiled
{
    public class Camera : GameComponent
    {
        public Vector2 position;
        public float zoom = 1.0f;
        public Camera(Game game) : base(game)
        {
            position = new Vector2(0, 0);
        }

    }
}
