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
            Mappings.actionMappings["move_left"].onActionMappingPressed += MoveLeft;
            Mappings.actionMappings["move_left"].onActionMappingReleased += LeftRightReleased;
            Mappings.actionMappings["move_right"].onActionMappingPressed += MoveRight;
            Mappings.actionMappings["move_right"].onActionMappingReleased += LeftRightReleased;
            Mappings.actionMappings["move_jump"].onActionMappingPressed += PlayerJump;
        }

        private void PlayerJump(ActionMappingArgs e)
        {
            ((Player)controlledEntity).Jump();
        }

        private void MoveRight(ActionMappingArgs e)
        {
            inputLR = 1.0f;
        }

        private void LeftRightReleased(ActionMappingArgs e)
        {
            inputLR = 0.0f;
        }

        private void MoveLeft(ActionMappingArgs e)
        {
            inputLR = -1.0f;
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
            }
        }
    }
}
