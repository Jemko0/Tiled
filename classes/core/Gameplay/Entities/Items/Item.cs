using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.DataStructures;
using Tiled.Gameplay.Items.ItemBehaviours;
using Tiled.ID;

namespace Tiled.Gameplay.Items
{
    public class EItem : Entity
    {
        public Item Item { get; set; }
        public EItemType type;
        public ushort count = 1;
        public bool canPickUp = true;
        public bool instaPickUpPrevention = true;
        public bool isSwing;
        public Entity? swingOwner;
        public float age;

        public delegate void swingEnd(ItemSwingArgs e);
        public event swingEnd swingEnded;

        private IItemBehaviour behavior;
        public EItem()
        {
        }

        public override void Initialize(EEntityType type)
        {
            //base.Initialize(type);
            return;
        }

        public override void Update()
        {
            base.Update();
            age += Main.delta;

            if (!isSwing)
            {
                velocity.X *= 0.9f;
                velocity.Y += World.gravity;
                
                if (age > 1)
                {
                    instaPickUpPrevention = false;
                }
                MovementUpdate();

                if (IsTouchingLocalPlayer() && canPickUp && !instaPickUpPrevention)
                {
                    if (((EPlayer)Program.GetGame().localPlayerController.controlledEntity).inventory.Add(new ContainerItem(type, count)))
                    {
                        Destroy();
                    }
                }
            }
            else
            {
                if(swingOwner != null)
                {
                    SwingItem(swingOwner);
                }
            }

#if TILEDSERVER
            velocity.X += 0.2f;
#endif
        }

        public void SwingItem(Entity entity)
        {
            position = entity.position;
            float animProgress = age / Item.useTime;

            switch(Item.swingAnimationType)
            {
                case EItemSwingAnimationType.None:
                    break;

                case EItemSwingAnimationType.Swing:
                    position = new Vector2(entity.facingLeft? entity.GetRect().Left : entity.GetRect().Right, entity.GetRect().Center.Y);
                    rotation = MathHelper.Lerp(entity.facingLeft? -5.0f : -4.0f, entity.facingLeft ? -10.0f : 1.0f, animProgress);
                    rotOrigin.X = -8.0f;
                    rotOrigin.Y = 40.0f;
                    break;
            }

            if(animProgress > 1.0f)
            {
                swingEnded?.Invoke(new ItemSwingArgs(type));
                Destroy();
            }
        }

        public bool IsTouchingLocalPlayer()
        {
            Entity? collide = collision.GetCollidingEntity();
            if(collide == null)
            {
                return false;
            }
            return collide == Program.GetGame().localPlayerController.controlledEntity;
        }

        public void InitWithID(EItemType type)
        {
            Item = ItemID.GetItem(type);
            this.type = type;
            entitySprite = Item.sprite;
            size = Item.size;
            behavior = ItemBehaviorFactory.CreateBehavior(type);
            RegisterCollisionComponent();
        }

        public static EItem CreateItem(EItemType type)
        {
            EItem newItem = new EItem();
            newItem.InitWithID(type);
            Main.RegisterEntity(newItem);
            return newItem;
        }

        public override void Draw(ref SpriteBatch sb)
        {
            base.Draw(ref sb);
            //sb.DrawString(Fonts.Andy_24pt, count.ToString(), new Vector2(Rendering.WorldToScreen(GetRect()).X, Rendering.WorldToScreen(GetRect()).Y), Color.Green);
        }

        public void Use()
        {
            behavior?.Use(this);
        }

        public void Use(int? tileX = null, int? tileY = null, Entity usingEntity = null)
        {
            if(usingEntity != null)
            {
                UseWithEntity(usingEntity);
            }

            if(tileX != null && tileY != null)
            {
                UseOnTile((int)tileX, (int)tileY);
            }
        }

        /// <summary>
        /// this is called before UseWithTile(int x, int y)
        /// </summary>
        /// <param name="entity"></param>
        public void UseWithEntity(object entity)
        {
            if (Item.consumable && behavior.CanConsume(this))
            {
                ((EPlayer)entity).inventory.RemoveFromSlot(((EPlayer)entity).selectedSlot, 1);
            }

            behavior?.UseWithEntity(this, entity);
        }

        public void UseOnTile(int x, int y)
        {
            behavior?.UseOnTile(this, x, y);
        }
    }
}
