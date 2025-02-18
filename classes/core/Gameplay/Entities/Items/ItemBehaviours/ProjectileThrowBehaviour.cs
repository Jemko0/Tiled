using System;
using Tiled.Gameplay.Entities.Projectiles;

namespace Tiled.Gameplay.Items.ItemBehaviours
{
    public class ProjectileThrowBehaviour : IItemBehaviour
    {
        public bool CanConsume(EItem item)
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
            EProjectile projectile = EProjectile.CreateProjectile(item.Item.projectile, (Entity)entity);
            projectile.position = ((Entity)entity).position;
        }
    }
}
