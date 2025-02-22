using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        public Texture2D entitySprite;
        protected CollisionComponent collision;
        protected int frameSlotSizeX = 32;
        protected int frameSlotSizeY = 48;
        protected float rotation = 0.0f;
        protected Vector2 rotOrigin = new(0, 0);
        public int netID = -1;

        public bool facingLeft;
        public int direction;
        public Entity()
        {
            RegisterCollisionComponent();
        }

        public void RegisterCollisionComponent()
        {
            collision = new CollisionComponent(this);
        }

        public static T NewEntity<T>(params object?[]? args) where T : Entity
        {
            T newEntity = (T)Activator.CreateInstance(typeof(T), args);
            newEntity.Initialize(EEntityType.None);
            Main.RegisterEntity(newEntity);
            newEntity.Begin();
            return newEntity;
        }

        public void Destroy()
        {
            if (Main.netMode == ENetMode.Standalone)
            {
                LocalDestroy();
            }

            if (Main.netMode == ENetMode.Server)
            {

            }

            if (Main.netMode == ENetMode.Client)
            {

            }
        }

        public void LocalDestroy()
        {
            Main.UnregisterEntity(this);
            Dispose();
        }

        /// <summary>
        /// use this to setup action mappings etc...
        /// </summary>
        /// <param name="playerController"></param>
        public virtual void Possessed(Controller playerController)
        {
            return;
        }

        /// <summary>
        /// handles sprite and hitbox, ONLY OVERRIDE IF YOU KNOW WHAT YOU ARE DOING
        /// </summary>
        /// <param name="type"></param>
        public virtual void Initialize(EEntityType type)
        {
            EntityDef e = EntityID.GetEntityInfo(type);
            size = e.size;
            entitySprite = e.sprite;
        }

        public virtual System.Drawing.RectangleF GetRectF()
        {
            return new System.Drawing.RectangleF(position.X, position.Y, size.X, size.Y);
        }

        public virtual Rectangle GetRect()
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
        }

        public virtual void Begin()
        {

        }

        /// <summary>
        /// default update handles facing left/right
        /// </summary>
        public virtual void Update()
        {
            if(velocity.X != 0.0f)
            {
                facingLeft = velocity.X < 0.0f;
                direction = facingLeft ? -1 : 1;
            }
        }

        public virtual Rectangle? GetFrame()
        {
            return null;
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
            Color finalColor = Color.White;

            if(!World.IsValidIndex(World.lightMap, (int)(position.X / World.TILESIZE), (int)(position.Y / World.TILESIZE)))
            {
                finalColor = Color.Black;
                goto draw;
            }

            finalColor *= (float)(World.lightMap[(int)(position.X / World.TILESIZE), (int)(position.Y / World.TILESIZE)] / (float)Lighting.MAX_LIGHT);
            finalColor.A = 255;

            draw:
            SpriteEffects flip = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sb.Draw(entitySprite, Rendering.WorldToScreen(GetRect()), GetFrame(), finalColor, rotation, rotOrigin, flip, 0u);
        }

        public void Dispose()
        {
            position = Vector2.Zero;
            velocity = Vector2.Zero;
        }
    }
}