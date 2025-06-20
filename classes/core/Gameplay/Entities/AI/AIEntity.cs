using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled.Gameplay.Entities
{
    public class AIEntity : Entity
    {
        protected UInt64 timer;
        public float inputLR = 0;
        public bool hasGravity = false;

        public float accel = 0.25f;
        public float maxWalkSpeed = 2.0f;

        public override void Update()
        {
            base.Update();
            UpdateAI();
            MovementUpdate();
        }

        public override void ApplyDamage(uint damage, int fromNetID)
        {
            healthComponent.ApplyDamage(damage, fromNetID);
        }

        public void UpdateTimer()
        {
            timer++;
        }

        public virtual void UpdateAI()
        {

        }

        public void AddInput(float input)
        {
            inputLR = input;
        }

        public float ConsumeInput()
        {
            float t = inputLR;
            inputLR = 0;

            return t;
        }

        public override void MovementUpdate()
        {
            float input = ConsumeInput();

            velocity.X = Math.Clamp(velocity.X + (accel * input), -maxWalkSpeed, maxWalkSpeed);

            if(hasGravity)
            {
                velocity.Y += World.gravity;
            }

            if (input == 0)
            {
                velocity.X *= 0.8f;
            }

            base.MovementUpdate();
        }
    }
}
