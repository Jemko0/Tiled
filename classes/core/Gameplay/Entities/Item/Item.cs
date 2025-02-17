using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tiled.DataStructures;
using Tiled.ID;

namespace Tiled.Gameplay.Items
{
    public class EItem : Entity
    {
        public Item Item { get; set; }
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

            velocity.X *= 0.9f;
            velocity.Y += World.gravity;
            MovementUpdate();

            if(IsTouchingLocalPlayer())
            {
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

            entitySprite = Item.sprite;
            size = Item.size;
            RegisterCollisionComponent();
        }

        public static EItem CreateItem(EItemType type)
        {
            EItem newItem = new EItem();
            newItem.InitWithID(type);
            Main.RegisterEntity(newItem);
            return newItem;
        }
    }
}
