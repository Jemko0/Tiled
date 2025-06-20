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
using Tiled.Events;
using Tiled.Gameplay.Components;
using Tiled.UI.Widgets;
using Tiled.Gameplay.Entities.AI;

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
        public UWEscapeMenu escMenu;
        public UWSettings settingsWidget;
        
        public WProgressBar healthBarUI;

        public EPlayer() : base() { }

        public override void Begin()
        {
            base.Begin();
        }

        public override void Possessed(Controller playerController)
        {
#if !TILEDSERVER
            Mappings.actionMappings["move_jump"].onActionMappingPressed += JumpPressed;
            Mappings.actionMappings["move_jump"].onActionMappingReleased += JumpReleased;
            Mappings.actionMappings["inv_1"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_2"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_3"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_4"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_5"].onActionMappingPressed += SetSlot;
            Mappings.actionMappings["inv_open"].onActionMappingPressed += OpenInventory;
            Mappings.actionMappings["esc_menu"].onActionMappingPressed += OpenEsc;
            Mappings.actionMappings["dbg_selfdmg"].onActionMappingPressed += dbgSelfDamage;
            Mappings.actionMappings["time_fwd"].onActionMappingPressed += TimeFwd;
            Mappings.actionMappings["time_bwd"].onActionMappingPressed += TimeBwd;
            InputManager.onLeftMousePressed += LMB;
            InputManager.onRightMousePressed += RMB;



            if(Main.netMode == ENetMode.Standalone)
            {
                inventory = new Container(52);
                inventory.entityCarrier = this;

                inventory.items[0] = new ContainerItem(EItemType.BasePickaxe, 1);
                inventory.items[1] = new ContainerItem(EItemType.BaseAxe, 1);
                inventory.items[2] = new ContainerItem(EItemType.Torch, 99);
                inventory.items[3] = new ContainerItem(EItemType.StoneBlock, 999);
                inventory.items[10] = new ContainerItem(EItemType.Bomb, 1000);

                ClientCreateUI();
            }

            if(Main.netMode == ENetMode.Client)
            {
                Main.netClient.RequestInventory();
            }
#else
#endif
        }

        private void TimeBwd(ActionMappingArgs e)
        {
            Program.GetGame().world.worldTime -= 100 * Main.delta;
        }

        private void TimeFwd(ActionMappingArgs e)
        {
            Program.GetGame().world.worldTime += 100 * Main.delta;
        }

        private void dbgSelfDamage(ActionMappingArgs e)
        {
            healthComponent.ApplyDamage(10, clientID);
        }

        public override void ApplyDamage(uint damage, int fromNetID)
        {
            healthComponent.ApplyDamage(damage, fromNetID);
        }

        private void DamageReceived(DamageEventArgs e)
        {
            healthBarUI.value = healthComponent.health;
        }

        private void ClientCreateUI()
        {
            inventoryUI = HUD.CreateWidget<UWContainerWidget>(Program.GetGame().localHUD);
            inventoryUI.SetGeometry(new Vector2(400, 100), AnchorPosition.BottomCenter, new(0, -50));
            inventoryUI.SetContainer(ref inventory);
            inventoryUI.UpdateSlots();

            healthBarUI = HUD.CreateWidget<WProgressBar>(Program.GetGame().localHUD);
            healthBarUI.minValue = 0.0f;
            healthBarUI.value = healthComponent.health;
            healthBarUI.maxValue = healthComponent.maxHealth;
            healthBarUI.backgroundColor = Color.DarkRed;
            healthComponent.onDamageGet += DamageReceived;

            invOpen = true;
            OpenInventory(new ActionMappingArgs(Keys.None));
        }

        private void OpenInventory(ActionMappingArgs e)
        {
            invOpen = !invOpen;
            //Debug.WriteLine(invOpen);
            Program.GetGame().localPlayerController.inUI = invOpen;
            inventoryUI.SetOpenInv(invOpen);

            if(!invOpen)
            {
                inventoryUI.SetGeometry(new Vector2(400, 100), AnchorPosition.BottomCenter, new(0, -25));
                healthBarUI.SetGeometry(new Vector2(320, 32), AnchorPosition.BottomCenter, new(-40, -135));
                healthBarUI.visible = true;
            }
            else
            {
                inventoryUI.SetGeometry(new Vector2(400, 540), AnchorPosition.Center, new(0, 0));
                healthBarUI.SetGeometry(new Vector2(320, 32), AnchorPosition.BottomCenter, new(-40, -135));
                healthBarUI.visible = false;
            }
        }

        private void OpenEsc(ActionMappingArgs e)
        {
            Main.escMenuOpen = !Main.escMenuOpen;
            if(Main.escMenuOpen)
            {
                escMenu = HUD.CreateWidget<UWEscapeMenu>(Program.GetGame().localHUD);
                escMenu.SetGeometry(new Vector2(1920, 1080), AnchorPosition.Center);
            }
            else
            {
                if(settingsWidget != null)
                {
                    settingsWidget.DestroyWidget();
                }

                escMenu.DestroyWidget();
            }
        }

        public void ClientInventoryReceived()
        {
            ClientCreateUI();
        }

        private void SetSlot(ActionMappingArgs e)
        {
            if(invOpen || Main.escMenuOpen)
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

            if(Keyboard.GetState().IsKeyDown(Keys.D9))
            {
                Program.GetGame().world.worldTime += 0.1f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D8))
            {
                Program.GetGame().world.worldTime -= 0.1f;
            }
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

        public override void Destroyed()
        {
#if !TILEDSERVER
            inventory.items = null;
            inventory = null;
            InputManager.onLeftMousePressed -= LMB;
            InputManager.onRightMousePressed -= RMB;
            foreach(var mapping in Mappings.actionMappings)
            {
                EventHelper.UnbindAllEventHandlers(mapping.Value, "onActionMappingPressed");
                EventHelper.UnbindAllEventHandlers(mapping.Value, "onActionMappingReleased");
            }
            inventoryUI.DestroyWidget();

            EventHelper.UnbindAllEventHandlers(healthComponent, "onDamageGet");
            healthComponent = null;
            healthBarUI.backgroundTexture.Dispose();
            healthBarUI.fillTexture.Dispose();
            healthBarUI.DestroyWidget();
#endif
        }

        private void LMB(MouseButtonEventArgs e)
        {
            if (!Program.GetGame().IsActive || invOpen || Main.escMenuOpen)
            {
                return;
            }

            Point tile = Rendering.ScreenToTile(Mouse.GetState().Position);

            if(Main.netMode == ENetMode.Standalone)
            {
                SwingItem(selectedSlot, tile);
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

            var en = Entity.NewEntity<AIECow>();
            en.Initialize(EEntityType.Cow);
            en.position = position;
        }

        public void Jump()
        {
            velocity.Y = -jumpPower;
        }
    }
}
