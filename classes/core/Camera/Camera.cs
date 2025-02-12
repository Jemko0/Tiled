using Microsoft.Xna.Framework;

namespace Tiled
{
    public class Camera : GameComponent
    {
        Vector2 position;

        public Camera(Game game) : base(game)
        {
            position = new Vector2(0, 0);
        }

    }
}
