using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Tiled.Collision;
using Tiled.DataStructures;
using Tiled.Gameplay.Items;
using Tiled.Input;
using Tiled.Inventory;
using Tiled.UI.UserWidgets;
using Tiled.UI;
using Tiled.ID;
using System.Diagnostics;

namespace Tiled.Gameplay
{
    public class EPlayer : Entity
    {
        public float accel = 0.25f;
        public float maxWalkSpeed = 3.0f;
        public float jumpPower = 6f;
        public Container inventory;
        int jumpCounter = 0;
        public int selectedSlot = 0;
        public int clientID = -1;
        public bool canUseItems = true;

        public bool invOpen;
        UWContainerWidget inventoryUI;
        public EPlayer()
        {
            collision = new CollisionComponent(this);
        }

        public override void Begin()
        {
            base.Begin();
        }

        public override void Possessed(Controller playerController)
        {
            Mappings.actionMappings["move_jump"].onActionMappingPressed += JumpPressed;
            Mappings.actionMappings["move_jump"].onActionMappingReleased += JumpReleased;
            Mappings.actionMappings["inv_1"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_2"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_3"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_4"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_5"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_open"].onActionMappingPressed += OpenInventory;

            InputManager.onLeftMousePressed += LMB;
            InputManager.onRightMousePressed += RMB;

#if !TILEDSERVER
            if(Main.netMode == ENetMode.Standalone)
            {
                inventory = new Container(52);
                inventory.entityCarrier = this;

                inventoryUI = HUD.CreateWidget<UWContainerWidget>(Program.GetGame().localHUD);
                inventoryUI.SetGeometry(new Vector2(400, 100), AnchorPosition.TopLeft, new(25, 25));
                inventoryUI.SetContainer(ref inventory);
                inventoryUI.UpdateSlots();

                inventory.items[0] = new ContainerItem(EItemType.BasePickaxe, 1);
                inventory.items[1] = new ContainerItem(EItemType.BaseAxe, 1);
                inventory.items[2] = new ContainerItem(EItemType.Torch, 99);
                inventory.items[3] = new ContainerItem(EItemType.Bomb, 16);
                inventory.items[4] = new ContainerItem(EItemType.StoneBlock, 999);
            }

            if(Main.netMode == ENetMode.Client)
            {
                Main.netClient.RequestInventory();
            }
#else
            
#endif
        }

        private void OpenInventory(ActionMappingArgs e)
        {
            invOpen = !invOpen;
            Debug.WriteLine(invOpen);
            Program.GetGame().localPlayerController.inUI = invOpen;
            inventoryUI.SetOpenInv(invOpen);
        }

        public void ClientInventoryReceived()
        {
            inventoryUI = HUD.CreateWidget<UWContainerWidget>(Program.GetGame().localHUD);
            inventoryUI.SetGeometry(new Vector2(400, 100), AnchorPosition.TopLeft, new(25, 25));
            inventoryUI.SetContainer(ref inventory);
            inventoryUI.UpdateSlots();
        }

        private void SetSlot(ActionMappingArgs e)
        {
            if(invOpen)
            {
                return;
            }

            int newSlot = -1;

            switch (e.key)
            {
                case Keys.D1:
                    newSlot = 0;
                    break;

                case Keys.D2:
                    newSlot = 1;
                    break;

                case Keys.D3:
                    newSlot = 2;
                    break;

                case Keys.D4:
                    newSlot = 3;
                    break;

                case Keys.D5:
                    newSlot = 4;
                    break;
            }

            selectedSlot = newSlot;

#if !TILEDSERVER
            if(Main.netMode == ENetMode.Client)
            {
                Main.netClient.SetSelectedSlot(newSlot);
            }
#endif
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

            if(Keyboard.GetState().IsKeyDown(Keys.P))
            {
                var i = EItem.CreateItem(EItemType.BasePickaxe);
                i.position = position;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.N))
            {
                var i = EItem.CreateItem(EItemType.Base);
                i.position = position;
            }
        }

        public override void Update()
        {
            base.Update();

            float inputLR = Program.GetGame().localPlayerController.inputLR;

            velocity.Y += World.gravity;

            //if this player is not the local player
            if (Program.GetGame().localPlayerController.controlledEntity != this)
            {
                MovementUpdate();
                return;
            }

            velocity.X = Math.Clamp(velocity.X + (accel * inputLR), -maxWalkSpeed, maxWalkSpeed);

            if(inputLR == 0)
            {
                velocity.X *= 0.8f;
            }
            
            if(Mappings.IsMappingHeld("move_jump") && jumpCounter < 15)
            {
                Jump();
                jumpCounter++;
            }
            
            MovementUpdate();
        }

        public override Rectangle? GetFrame()
        {
            const byte fH = 48;
            const byte fW = 24;
            int fI = 0;
            int walkI = 0;

            walkI = Main.runtime % 0.2f > 0.1f? 1 : 2;

            fI = velocity.Y != 0? 0 : (Math.Abs(velocity.X) > 0.01f? walkI : 1);

            return new Rectangle(0, fH * fI, fW, fH);
        }

        private void LMB(MouseButtonEventArgs e)
        {
            if (!Program.GetGame().IsActive || invOpen)
            {
                return;
            }

            Point tile = Rendering.ScreenToTile(Mouse.GetState().Position);

            if(Main.netMode == ENetMode.Standalone)
            {
                SwingItem(selectedSlot, tile);
            }

            if(Keyboard.GetState().IsKeyDown(Keys.F))
            {
                Program.GetGame().localPlayerController.attachToEntity = false;
            }

#if !TILEDSERVER
            RepSwingItem(tile);
#endif
        }

#if !TILEDSERVER
        public void RepSwingItem(Point tile)
        {
            if (Main.netMode == ENetMode.Client)
            {
                Main.netClient.RequestItemSwing(tile);
                SwingItem(selectedSlot, tile);
            }

            if(Main.netMode == ENetMode.Standalone)
            {
                SwingItem(selectedSlot, Rendering.ScreenToTile(Mouse.GetState().Position));
            }
        }
#endif

        /// <summary>
        /// in multiplayer instances, the server will execute this
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="tile"></param>
        public void SwingItem(int slot, Point tile)
        {
            if(Main.netMode != ENetMode.Server)
            {
                if (!canUseItems || inventory.items[slot].type == EItemType.None)
                {
                    return;
                }
            }

            canUseItems = false;
            var swingItem = EItem.CreateItem(inventory.items[selectedSlot].type);
            swingItem.swingEnded += CurrentSwingItemSwingEnded;
            swingItem.direction = direction;
            swingItem.isSwing = true;
            swingItem.swingOwner = this;

            if(Main.netMode != ENetMode.Client)
            {
                swingItem.Use();
                swingItem.UseWithEntity(this, tile);
                swingItem.UseOnTile(tile.X, tile.Y);
            }
        }

        private void CurrentSwingItemSwingEnded(ItemSwingArgs e)
        {
            canUseItems = true;
#if !TILEDSERVER
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && ItemID.GetItem(e.type).autoReuse)
            {
                RepSwingItem(Rendering.ScreenToTile(Mouse.GetState().Position));
            }
#endif
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
