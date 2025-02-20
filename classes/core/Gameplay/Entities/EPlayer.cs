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

namespace Tiled.Gameplay
{
    public class EPlayer : Entity
    {
        public float accel = 0.75f;
        public float maxWalkSpeed = 4.0f;
        public float jumpPower = 6f;
        public Container inventory;
        int jumpCounter = 0;
        public int selectedSlot = 0;
        public int clientID = -1;
        public bool canUseItems = true;
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

            InputManager.onLeftMousePressed += LMB;
            InputManager.onRightMousePressed += RMB;

            inventory = new Container(5);
            inventory.entityCarrier = this;

            inventoryUI = HUD.CreateWidget<UWContainerWidget>(Program.GetGame().localHUD);
            inventoryUI.SetGeometry(new Vector2(400, 100), AnchorPosition.TopLeft, new(25, 25));
            inventoryUI.SetContainer(ref inventory);
            inventoryUI.UpdateSlots();

            inventory.items[0] = new ContainerItem(EItemType.BasePickaxe, 1);
            inventory.items[1] = new ContainerItem(EItemType.DirtBlock, 999);
            inventory.items[2] = new ContainerItem(EItemType.Torch, 99);
            inventory.items[3] = new ContainerItem(EItemType.Bomb, 16);
        }

        private void SetSlot(ActionMappingArgs e)
        {
            switch (e.key)
            {
                case Keys.D1:
                    selectedSlot = 0;
                    return;

                case Keys.D2:
                    selectedSlot = 1;
                    return;

                case Keys.D3:
                    selectedSlot = 2;
                    return;

                case Keys.D4:
                    selectedSlot = 3;
                    return;

                case Keys.D5:
                    selectedSlot = 4;
                    return;
            }
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
            velocity.Y += World.gravity;

            if(Mappings.IsMappingHeld("move_jump") && jumpCounter < 15)
            {
                Jump();
                jumpCounter++;
            }
            
            MovementUpdate();

            //inventoryUI.UpdateChildren(ref inventory);
        }

        public override Rectangle? GetFrame()
        {
            frameSlotSizeX = 24;
            frameSlotSizeY = 48;

            if(velocity.Y == 0)
            {
                return new Rectangle(frameSlotSizeX * 0, frameSlotSizeY * 1, frameSlotSizeX, frameSlotSizeY);
            }
            return new Rectangle(frameSlotSizeX * 0, frameSlotSizeY * 0, frameSlotSizeX, frameSlotSizeY);
        }

        private void LMB(MouseButtonEventArgs e)
        {
            if (!Program.GetGame().IsActive)
            {
                return;
            }

            Point tile = Rendering.ScreenToTile(Mouse.GetState().Position);

            SwingItem(selectedSlot, tile);
        }

        public void SwingItem(int slot, Point tile)
        {
            if(!canUseItems || inventory.items[slot].type == EItemType.None)
            {
                return;
            }

            canUseItems = false;
            var swingItem = EItem.CreateItem(inventory.items[selectedSlot].type);
            swingItem.swingEnded += CurrentSwingItemSwingEnded;
            swingItem.isSwing = true;
            swingItem.swingOwner = this;
            swingItem.Use();
            swingItem.UseWithEntity(this);
            swingItem.UseOnTile(tile.X, tile.Y);
        }

        private void CurrentSwingItemSwingEnded(ItemSwingArgs e)
        {
            canUseItems = true;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && ItemID.GetItem(e.type).autoReuse)
            {
                SwingItem(selectedSlot, Rendering.ScreenToTile(Mouse.GetState().Position));
            }
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
