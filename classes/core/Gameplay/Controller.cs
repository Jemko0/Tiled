using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.DataStructures;
using Tiled.Input;

namespace Tiled.Gameplay
{
    public class Controller
    {
        public bool attachToEntity = true;
        public float inputLR;
        public float inputUD;

        public Controller()
        {

        }

        public Entity? controlledEntity;

        public void Possess(Entity entity)
        {
            controlledEntity = entity;
        }

        public void Unpossess()
        {
            controlledEntity = null;
        }

        public void Update()
        {
            if (controlledEntity != null)
            {
                if(attachToEntity)
                {
                    Program.GetGame().localCamera.position = controlledEntity.position;
                }

                inputLR = 0.0f;

                if(Mappings.IsMappingHeld("move_left"))
                {
                    inputLR = -1.0f;
                }

                if (Mappings.IsMappingHeld("move_right"))
                {
                    inputLR = 1.0f;
                }

                if(Mappings.IsMappingHeld("move_left") && Mappings.IsMappingHeld("move_right"))
                {
                    inputLR = 0.0f;
                }
            }
        }
    }
}
