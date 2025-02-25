using Microsoft.Xna.Framework;
using System;
using Tiled.DataStructures;
using Tiled.Gameplay.Entities.Projectiles;
using Tiled.ID;

namespace Tiled.Gameplay.Projectiles.ProjectileBehaviours
{
    public interface IProjectileBehavior
    {
        void Start(EProjectile item);
        void Update(EProjectile item, float delta);
        void Hit(EProjectile item, Vector2 hitVelocity);
        void HitEntity(EProjectile item, Entity entity);
    }

    public class DefaultProjectileBehaviour : IProjectileBehavior
    {
        public void Hit(EProjectile item, Vector2 hitVelocity)
        {
            throw new NotImplementedException();
        }

        public void HitEntity(EProjectile item, Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Start(EProjectile item)
        {
            throw new NotImplementedException();
        }

        public void Update(EProjectile item, float delta)
        {
            throw new NotImplementedException();
        }
    }

    public class BombProjectileBehaviour : IProjectileBehavior
    {
        float age = 0.0f;
        float rotAmt = 0.0f;
        public void Hit(EProjectile item, Vector2 hitVelocity)
        {
            float restitution = 0.7f;
            item.velocity.Y = -hitVelocity.Y * restitution;
        }

        public void HitEntity(EProjectile item, Entity entity)
        {
            
        }

        public void Start(EProjectile item)
        {
            item.centerSprite = true;
            item.rotOrigin = new(8, 8);
            age = 0.0f;
        }

        public void Update(EProjectile item, float delta)
        {
            float frictionMultiplier = 0.98f;

            item.velocity.Y += World.gravity;

            if(item.collision.IsOnGround())
            {
                item.velocity.X *= frictionMultiplier;
                rotAmt = item.velocity.X / 8.0f;
            }
            else
            {
                rotAmt *= frictionMultiplier;
            }

            item.rotation += rotAmt;

            if(Main.netMode == ENetMode.Client)
            {
                return;
            }

            age += delta;

            if(age > 3.0f)
            {
                World.CreateExplosion((int)(item.position.X / World.TILESIZE), (int)(item.position.Y / World.TILESIZE), 15, 50, 50);
                item.Destroy();
            }
        }
    }

    public static class ProjectileBehaviorFactory
    {
        public static IProjectileBehavior CreateBehavior(EProjectileType type)
        {
            var template = ProjectileID.GetProjectile(type);
            if (template.behaviourType != null)
            {
                return (IProjectileBehavior)Activator.CreateInstance(template.behaviourType);
            }
            return new DefaultProjectileBehaviour();
        }
    }
}
