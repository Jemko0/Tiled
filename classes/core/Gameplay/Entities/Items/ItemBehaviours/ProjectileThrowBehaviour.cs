using Microsoft.Xna.Framework;
using Tiled.DataStructures;
using Tiled.Gameplay.Entities.Projectiles;
using Tiled.Networking.Shared;
namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class ProjectileThrowBehaviour : IItemBehaviour
    {
        public bool CanConsume(EItem item, Point tile)
        {
            return true;
        }

        public void Use(EItem item)
        { 
        }

        public void UseOnTile(EItem item, int x, int y)
        {
        }

        public void UseWithEntity(EItem item, object entity)
        {
            EProjectile projectile;
            if (Main.netMode == ENetMode.Standalone)
            {
                projectile = EProjectile.CreateProjectile(item.Item.projectile, (Entity)entity);
                projectile.position = ((Entity)entity).position;
                projectile.velocity = (item.Item.projectileThrowVelocity * item.swingOwner.direction);
            }
#if TILEDSERVER
            Main.netServer.ServerSpawnEntity(ENetEntitySpawnType.Projectile, EEntityType.None, EItemType.None, item.Item.projectile, item.swingOwner.position, (item.Item.projectileThrowVelocity * item.swingOwner.direction));
#endif
        }
    }
}
