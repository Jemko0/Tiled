using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;

namespace Tiled.Gameplay.Entities.AI
{
    public class AIECow : AIEntity
    {
        int cycle = 0;

        public override void Initialize(EEntityType type)
        {
            base.Initialize(type);
            hasGravity = true;
        }

        public override void UpdateAI()
        {
            base.UpdateAI();

            if(timer % 60 == 0)
            {
                cycle++;
            }

            if (cycle > 10)
            {
                SetupMovement();
                cycle = 0;
            }

            MovementTick();
        }

        public override Rectangle? GetFrame()
        {
            int frameHeight = entitySprite.Height / 3;

            Rectangle r = new Rectangle();
            r.Width = entitySprite.Width;
            r.Height = frameHeight;

            int h = Math.Abs(velocity.X) > 0? (int)((Main.runtime % 1f) * 3.0f) : 0;
            r.Y = frameHeight * h;

            return r;
        }

        float movementDir = 0.0f;

        public void SetupMovement()
        {
            movementDir = (float)Math.Round((new Random().NextSingle() * 2) - 1);
        }

        public void MovementTick()
        {
            AddInput(movementDir);
        }
    }
}
