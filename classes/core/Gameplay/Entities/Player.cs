using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Collision;
using Tiled.DataStructures;
using Tiled.Input;

namespace Tiled.Gameplay
{
    public class Player : Entity
    {
        public float accel = 0.75f;
        public float maxWalkSpeed = 5.0f;
        public float jumpPower = 15.0f;
        public Player()
        {
            collision = new CollisionComponent(this);
            InputManager.onLeftMousePressed += LMB;
            InputManager.onRightMousePressed += RMB;
        }

        public override void Update()
        {
            base.Update();
            float inputLR = Program.GetGame().localPlayerController.inputLR;
            velocity.X = Math.Clamp(velocity.X + (accel * inputLR), -maxWalkSpeed, maxWalkSpeed);

            if(inputLR == 0)
            {
                velocity.X *= 0.8f;
            }
            velocity.Y += 0.66f;

            MovementUpdate();
        }

        private void LMB(MouseButtonEventArgs e)
        {
            Point tile = Rendering.ScreenToTile(e.position);

            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                World.SetTile(tile.X, tile.Y, ETileType.Torch);
                return;
            }

            World.SetTile(tile.X, tile.Y, ETileType.Air);
        }

        private void RMB(MouseButtonEventArgs e)
        {
            Point tile = Rendering.ScreenToTile(e.position);
            World.SetWall(tile.X, tile.Y, EWallType.Air);
        }

        public void Jump()
        {
            if(collision.IsOnGround())
            {
                velocity.Y = -jumpPower;
            }
        }
    }
}
