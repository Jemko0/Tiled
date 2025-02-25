
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Tiled.DataStructures;
using Tiled.Gameplay.Items.ItemBehaviours;
using Tiled.Gameplay.Projectiles.ProjectileBehaviours;
using Tiled.ID;

namespace Tiled.Gameplay.Entities.Projectiles
{
    public class EProjectile : Entity
    {
        public Projectile Projectile { get; set; }
        public EProjectileType type;
        public Entity owner;

        public IProjectileBehavior behavior;
        public EProjectile()
        {
        
        }

        public void InitWithID(EProjectileType type)
        {
            Projectile = ProjectileID.GetProjectile(type);
            this.type = type;
            entitySprite = Projectile.sprite;
            size = Projectile.size;
            behavior = ProjectileBehaviorFactory.CreateBehavior(type);

            RegisterCollisionComponent();
            collision.onHit += Collision_onHit;
            collision.onEntityHit += Collision_onEntityHit;

            behavior?.Start(this);
        }

        private void Collision_onEntityHit(Entity e)
        {
            behavior?.HitEntity(this, e);
        }

        private void Collision_onHit(Vector2 hitVelocity)
        {
            behavior?.Hit(this, hitVelocity);
        }

        public static EProjectile CreateProjectile(EProjectileType type, Entity? ownerEntity = null)
        {
            EProjectile proj = new EProjectile();
            if(ownerEntity != null)
            {
                proj.owner = ownerEntity;
            }
            proj.InitWithID(type);
            Main.RegisterEntity(proj);
            return proj;
        }

        public override void Update()
        {
            base.Update();
            behavior.Update(this, Main.delta);
            MovementUpdate();
        }
    }
}
