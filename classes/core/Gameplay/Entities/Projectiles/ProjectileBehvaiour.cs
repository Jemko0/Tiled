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
        void Hit(EProjectile item, Vector2 hitVelocity, Vector2 hitNormal);
        void HitEntity(EProjectile item, Entity entity);
    }

    public class DefaultProjectileBehaviour : IProjectileBehavior
    {
        public void Hit(EProjectile item, Vector2 hitVelocity, Vector2 hitNormal)
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
