using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Collision;

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

        public void Jump()
        {
            if(collision.IsOnGround())
            {
                velocity.Y = -jumpPower;
            }
        }
    }
}
