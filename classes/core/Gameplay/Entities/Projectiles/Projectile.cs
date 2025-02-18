
using Tiled.DataStructures;
using Tiled.ID;

namespace Tiled.Gameplay.Entities.Projectiles
{
    public class EProjectile : Entity
    {
        public Projectile Projectile { get; set; }
        public EProjectileType type;
        public Entity owner;
        public EProjectile()
        {
        
        }

        public void InitWithID(EProjectileType type)
        {
            Projectile = ProjectileID.GetProjectile(type);
            this.type = type;
            entitySprite = Projectile.sprite;
            size = Projectile.size;

            RegisterCollisionComponent();
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

            MovementUpdate();
        }
    }
}
