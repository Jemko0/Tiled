using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tiled.DataStructures;
using Tiled.Input;

namespace Tiled.Gameplay
{
    public class Controller
    {
        public bool ignoreInput = false;
        public bool attachToEntity = true;
        public float inputLR;
        public float inputUD;
        public bool inUI = false;
        public Controller()
        {

        }

#nullable enable
        public Entity? controlledEntity;
        public void Possess(Entity entity)
        {
            controlledEntity = entity;
            entity.Possessed(this);
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

                if(ignoreInput)
                {
                    return;
                }
                GetInput();
            }
        }

        public void GetInput()
        {
            if (controlledEntity != Program.GetGame().localPlayerController.controlledEntity || Main.escMenuOpen)
            {
                inputLR = 0.0f;
                return;
            }
            inputLR = 0.0f;
            inputUD = 0.0f;

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

            if(Keyboard.GetState().IsKeyDown(Keys.W))
            {
                inputUD = -1.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                inputUD = 1.0f;
            }
        }
    }
}
