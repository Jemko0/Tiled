using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
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
        public float maxWalkSpeed = 4.0f;
        public float jumpPower = 6f;

        int jumpCounter = 0;
        public Player()
        {
            collision = new CollisionComponent(this);
            InputManager.onLeftMousePressed += LMB;
            InputManager.onRightMousePressed += RMB;
        }

        public override void Begin()
        {
            base.Begin();
            Mappings.actionMappings["move_jump"].onActionMappingPressed += JumpPressed;
            Mappings.actionMappings["move_jump"].onActionMappingReleased += JumpReleased;
        }

        private void JumpReleased(ActionMappingArgs e)
        {
            jumpCounter = int.MaxValue;
        }

        private void JumpPressed(ActionMappingArgs e)
        {
            if(collision.IsOnGround())
            {
                jumpCounter = 0;
            }
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
            velocity.Y += World.gravity;

            if(Mappings.IsMappingHeld("move_jump") && jumpCounter < 15)
            {
                Jump();
                jumpCounter++;
            }

            MovementUpdate();
        }

        private void LMB(MouseButtonEventArgs e)
        {
            if (!Program.GetGame().IsActive)
            {
                return;
            }

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
            if (!Program.GetGame().IsActive)
            {
                return;
            }

            Point tile = Rendering.ScreenToTile(e.position);
            World.SetWall(tile.X, tile.Y, EWallType.Air);
        }

        public void Jump()
        {
            velocity.Y = -jumpPower;
        }
    }
}
