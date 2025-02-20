using Google.Protobuf.WellKnownTypes;
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

        public void StartMultiplayerUpdate()
        {
            Timer timer = new Timer(Main.SERVER_TICKRATE);
            timer.Elapsed += SendClientUpdate;
            timer.Start();
        }

        private void SendClientUpdate(object? sender, ElapsedEventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed#

            Program.GetGame().localClient.SendPacket(
                "clientUpdate", 
                new 
                { 
                    id = Program.GetGame().localClient.PlayerID,
                    x = controlledEntity.position.X,
                    y = controlledEntity.position.Y,
                    velX = (Math.Truncate(100 * controlledEntity.velocity.X) / 100),
                    velY = (Math.Truncate(100 * controlledEntity.velocity.Y) / 100)
                });

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
            if (controlledEntity != Program.GetGame().localPlayerController.controlledEntity)
            {
                inputLR = 0.0f;
                return;
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
