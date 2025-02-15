using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.DataAnnotations;
using Tiled.Collision;
using Tiled.DataStructures;
using Tiled.ID;
namespace Tiled.Gameplay
{
    public class Entity : IDisposable
    {
        public Vector2 position;
        public Vector2 size;
        public Vector2 velocity;
        public bool canCollide = true;

        protected CollisionComponent collision;

        public Entity()
        {
            collision = new CollisionComponent(this);
        }

        public static T NewEntity<T>(params object?[]? args) where T : Entity
        {
            T newEntity = (T)Activator.CreateInstance(typeof(T), args);
            newEntity.Initialize(EEntityType.None);
            Main.RegisterEntity(newEntity);
            return newEntity;
        }

        public void Destroy()
        {
            Main.UnregisterEntity(this);
            Dispose();
        }

        public void Initialize(EEntityType type)
        {
            EntityDef e = EntityID.GetEntityInfo(type);
            size = e.size;
        }

        public System.Drawing.RectangleF GetRectF()
        {
            return new System.Drawing.RectangleF(position.X, position.Y, size.X, size.Y);
        }

        public Rectangle GetRect()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public virtual void Begin()
        {

        }

        public virtual void Update()
        {
            
        }

        public virtual void MovementUpdate()
        {
            if (canCollide)
            {
                collision.Move();
            }
            else
            {
                position += velocity;
            }
        }

        public virtual void Draw(ref SpriteBatch sb)
        {
            Texture2D t = new Texture2D(Program.GetGame().GraphicsDevice, 1, 1);
            t.SetData(new Color[] { Color.White });
            Color finalColor = Color.White;
            finalColor *= World.lightMap[(int)(position.X / World.TILESIZE), (int)(position.Y / World.TILESIZE)] / Lighting.MAX_LIGHT;
            finalColor.A = 255;
            sb.Draw(t, Rendering.WorldToScreen(GetRect()), finalColor);
        }

        public void Dispose()
        {
            position = Vector2.Zero;
            velocity = Vector2.Zero;
        }
    }
}